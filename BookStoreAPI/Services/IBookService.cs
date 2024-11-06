using AutoMapper;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BookStoreAPI.Services
{
    public interface IBookService
    {
        Task<(bool IsSuccess, Book Book)> AddBook(BookDto bookDto);
        Task<(bool IsSuccess, string? result)> RentBook(int Id, string email);
        Task<bool> CancelRent(int bookId, string userMail);
    }

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public BookService(IBookRepository bookRepository,
            IMapper mapper, UserManager<AppUser> userManager)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<(bool IsSuccess, Book Book)> AddBook(BookDto bookDto)
        {
            var mappedBook = _mapper.Map<Book>(bookDto);
            return await _bookRepository.AddBook(mappedBook);
        }

        public async Task<bool> CancelRent(int bookId, string userMail)
        {
            var user = await _userManager.FindByEmailAsync(userMail);
            if (user is null) return false;

            return await _bookRepository.CancelRent(bookId, user.Id);
        }

        public async Task<(bool IsSuccess, string? result)> RentBook(int Id, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return (false, null);

            var canRent = await _bookRepository.CanRent(user.Id);
            try
            {
                if (canRent)
                {
                    var bookExists = await _bookRepository.GetBookByIdAsync(Id);
                    if (bookExists is null) return (false, "This book does not exists.");

                    var result = await _bookRepository.RentBook(user.Id, Id);
                    return (result, "Successfull rent.");
                }

                return (canRent, "Maximum rent reached.");
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }
    }
}