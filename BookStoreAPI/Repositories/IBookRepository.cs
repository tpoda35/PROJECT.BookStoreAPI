using AutoMapper;
using Azure;
using BookStoreAPI.Data;
using BookStoreAPI.Dtos;
using BookStoreAPI.Helpers;
using BookStoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace BookStoreAPI.Repositories
{
    public interface IBookRepository
    {
        Task<Book?> GetBookByIdAsync(int id);
        Task<PagedList<Book>?> GetBookByPagination(int page, int pageSize, string? searchTerm);
        Task<(bool IsSuccess, Book Book)> AddBook(Book book);
        Task<bool> DeleteBook(int id);
        Task<bool> UpdateBook(int Id, BookDto bookDto);
    }

    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly IBookCacheManager _bookCacheManager;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public BookRepository(ApplicationDbContext context,
            IMemoryCache memoryCache,
            IBookCacheManager bookCacheManager)
        {
            _context = context;
            _cache = memoryCache;

            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(40));
            _bookCacheManager = bookCacheManager;
        }

        public async Task ClearBookCache()
        {
            await _semaphore.WaitAsync();
            try
            {
                _bookCacheManager.ClearCacheKeys(_cache);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<(bool IsSuccess, Book Book)> AddBook(Book book)
        {
            _context.Books.Add(book);

            var saved = await _context.SaveChangesAsync() > 0;

            if (saved)
            {
                await ClearBookCache();
            }

            return (saved, book);
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<PagedList<Book>?> GetBookByPagination(int page, int pageSize, string? searchTerm)
        {
            var cacheKey = CacheKeys.GetBookListKey(page, pageSize, searchTerm);

            if (_cache.TryGetValue(cacheKey, out PagedList<Book>? cachedResponse))
            {
                return cachedResponse;
            }

            var query = _context.Books.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => EF.Functions.Like(b.Title.ToLower(), $"%{searchTerm}%"));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1)*pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedList<Book>(items, totalCount, page, pageSize);

            _cache.Set(cacheKey, response, _cacheOptions);

            await _semaphore.WaitAsync();
            try
            {
                _bookCacheManager.AddCacheKey(cacheKey);
            }
            finally
            {
                _semaphore.Release();
            }

            return response;
        }

        public async Task<bool> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book is null) return false;

            _context.Books.Remove(book);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBook(int Id, BookDto bookDto)
        {
            var book = await _context.Books.FindAsync(Id);
            if (book is null) return false;

            if (book.Title != bookDto.Title) book.Title = bookDto.Title;
            if (book.Description != bookDto.Description) book.Description = bookDto.Description;
            if (book.Pages != bookDto.Pages) book.Pages = bookDto.Pages;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}