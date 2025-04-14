using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
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

                // Проверяем, существует ли автор
                var author = _repository.Author.GetAuthorByName(bookDTO.AuthorName);
                if (author == null)
                {
                    // Если нет, создаём нового автора
                    author = new Author { AuthorName = bookDTO.AuthorName };
                    _repository.Author.CreateAuthor(author);
                    _repository.Save(); // Сохраняем в базе данных
                }

                // Проверяем, существует ли жанр
                var genre = _repository.Genre.GetGenreByName(bookDTO.GenreName);
                if (genre == null)
                {
                    // Если нет, создаём новый жанр
                    genre = new Genre { GenreName = bookDTO.GenreName };
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save(); // Сохраняем в базе данных
                }

                // Повторно получаем автора и жанр после сохранения, чтобы иметь их с ID
                author = _repository.Author.GetAuthorByName(bookDTO.AuthorName);
                genre = _repository.Genre.GetGenreByName(bookDTO.GenreName);

                // Проверяем, существует ли такая книга с данным названием, автором и жанром
                var existingBook = _repository.Book
                    .FindByCondition(b =>
                        b.Title.ToLower() == bookDTO.Title.ToLower() &&
                        b.AuthorID == author.Id &&
                        b.GenreID == genre.Id, trackChanges: false)
                    .FirstOrDefault();

                if (existingBook != null)
                {
                    return Conflict($"Книга \"{bookDTO.Title}\" уже добавлена в библиотеку.");
                }

                // Создаём сущность книги и связываем с автором и жанром
                var bookEntity = _mapper.Map<Book>(bookDTO);
                bookEntity.AuthorID = author.Id;
                bookEntity.GenreID = genre.Id;

                // Отключаем навигационные свойства, чтобы не пытаться заново вставить авторов и жанры
                bookEntity.Author = null;
                bookEntity.Genre = null;

                // Создаём книгу в базе данных
                _repository.Book.CreateBook(bookEntity);
                _repository.Save();

                // Маппим сущность книги обратно в DTO для возврата клиенту
                var bookToReturn = _mapper.Map<BookDTO>(bookEntity);

                return Ok(bookToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(CreateBook)} action: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }




    }
}
   