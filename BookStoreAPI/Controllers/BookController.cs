using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using BookStoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IBookRepository _bookRepository;

        public BookController(IBookService bookService, IBookRepository bookRepository)
        {
            _bookService = bookService;
            _bookRepository = bookRepository;
        }

        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetBook(int Id)
        {
            var book = await _bookRepository.GetBookByIdAsync(Id);
            if (book is null) return NotFound();

            return Ok(book);
        }

        [Authorize]
        [HttpGet("All")]
        public async Task<ActionResult<PagedList<Book>>> GetBooks(
            [FromQuery] int Page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _bookRepository.GetBookByPagination(Page, pageSize, searchTerm);
            if (result is null) return NotFound();

            return Ok(result);
        }

        [Authorize("AdminPolicy")]
        [HttpPost("AddBook")]
        public async Task<IActionResult> AddBook([FromBody] BookDto bookDto)
        {
            var result = await _bookService.AddBook(bookDto);
            if (result.IsSuccess is false) return BadRequest();

            return CreatedAtAction(nameof(AddBook), new { id = result.Book.Id }, result.Book);
        }

        [Authorize("AdminPolicy")]
        [HttpPut("UpdateBook/{Id}")]
        public async Task<IActionResult> UpdateBook(int Id, [FromBody] BookDto bookDto)
        {
            var result = await _bookRepository.UpdateBook(Id, bookDto);
            if (result is false) return BadRequest("Something went wrong.");

            return NoContent();
        }

        [Authorize("AdminPolicy")]
        [HttpDelete("DeleteBook/{Id}")]
        public async Task<IActionResult> DeleteBook(int Id)
        {
            var result = await _bookRepository.DeleteBook(Id);
            if (result is false) return BadRequest("Something went wrong.");

            return NoContent();
        }
    }
}
