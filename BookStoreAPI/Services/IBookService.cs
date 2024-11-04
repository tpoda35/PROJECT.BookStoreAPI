using AutoMapper;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;

namespace BookStoreAPI.Services
{
    public interface IBookService
    {
        Task<(bool IsSuccess, Book Book)> AddBook(BookDto bookDto);
    }

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        private readonly int bookLimit = 5;

        public BookService(IBookRepository bookRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<(bool IsSuccess, Book Book)> AddBook(BookDto bookDto)
        {
            var mappedBook = _mapper.Map<Book>(bookDto);
            return await _bookRepository.AddBook(mappedBook);
        }
    }
}
