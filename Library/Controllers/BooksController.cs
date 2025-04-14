using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Collections.Specialized.BitVector32;

namespace Library.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public BooksController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetBooks()
        {
            try
            {
                var books = _repository.Book.GetAllBooks(trackChanges: false);
                var booksDto = _mapper.Map<IEnumerable<BookDTO>>(books);
                return Ok(booksDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(GetBooks)} action {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public IActionResult CreateBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                if (bookDTO == null)
                {
                    _logger.LogError("BookDTO object sent from client is null.");
                    return BadRequest("BookDTO object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid model state for the BookDTO object");
                    return UnprocessableEntity(ModelState);
                }

                var bookEntity = _mapper.Map<Book>(bookDTO);

                _repository.Book.CreateBook(bookEntity);
                _repository.Save();

                var bookToReturn = _mapper.Map<BookDTO>(bookEntity);
                return Ok(bookToReturn);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(CreateBook)} action {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}