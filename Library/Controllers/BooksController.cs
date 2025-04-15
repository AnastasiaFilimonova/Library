using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Entities.Models;
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
                var result = books.Select(book => new
                {
                    book.Id,
                    book.Title,
                    AuthorName = book.Author?.AuthorName,
                    Status = book.ReadingStatusID == (int)ReadingStatusEnum.Read ? "Прочитана" : "Не прочитана"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в GetBooks: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound("Книга не найдена.");
            var dto = new BookDetailsDTO
            {
                Id = book.Id,
                Title = book.Title,
                AuthorName = book.Author?.AuthorName,
                GenreName = book.Genre?.GenreName,
                Image = book.Image,
                PageCount = book.PageCount,
                Annotation = book.Annotation,
                ReadingStatusID = book.ReadingStatusID,
                ReadingStatusName = book.ReadingStatusID == (int)ReadingStatusEnum.Read ? "Прочитана" : "Не прочитана",
                Rating = book.ReadingStatus?.Rating,
                Review = book.ReadingStatus?.Review,
                Quotes = book.ReadingStatus?.Quotes,
                StartReadingDate = book.ReadingStatus?.StartReadingDate,
                EndReadingDate = book.ReadingStatus?.EndReadingDate
            };
            return Ok(dto);
        }

        [HttpPost]
        public IActionResult CreateBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                if (bookDTO == null)
                {
                    _logger.LogError("BookDTO object is null.");
                    return BadRequest("Book data is null.");
                }
                var author = _repository.Author.GetAuthorByName(bookDTO.AuthorName, trackChanges: false);
                if (author == null)
                {
                    author = new Author { AuthorName = bookDTO.AuthorName };
                    _repository.Author.CreateAuthor(author);
                    _repository.Save();
                }
                var genre = _repository.Genre.GetGenreByName(bookDTO.GenreName, trackChanges: false);
                if (genre == null)
                {
                    genre = new Genre { GenreName = bookDTO.GenreName };
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save();
                }
                var existingBook = _repository.Book.FindByCondition(b => b.Title == bookDTO.Title && b.AuthorID == author.Id, trackChanges: false).FirstOrDefault();
                if (existingBook != null)
                {
                    return Conflict("Такая книга уже есть в вашей библиотеке.");
                }
                var book = new Book
                {
                    Title = bookDTO.Title,
                    AuthorID = author.Id,
                    GenreID = genre.Id,
                    Image = bookDTO.Image,
                    PageCount = bookDTO.PageCount,
                    Annotation = bookDTO.Annotation,
                    ReadingStatusID = null
                };

                _repository.Book.CreateBook(book);
                _repository.Save();

                return Ok(new
                {
                    book.Id,
                    Status = "Не прочитана",
                    Message = "Книга успешно добавлена в библиотеку!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в методе CreateBook: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPatch("{id}")]
        public IActionResult PatchBook(int id, [FromBody] BookUpdateDTO updateDto)
        {
            try
            {
                var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id);
                if (book == null)
                    return NotFound("Книга не найдена.");
                if (updateDto.ReadingStatusID.HasValue)
                {
                    book.ReadingStatusID = updateDto.ReadingStatusID;
                }
                bool isRead = book.ReadingStatusID == (int)ReadingStatusEnum.Read;
                if (!isRead)
                {
                    return BadRequest("Редактирование доступно только для прочитанных книг.");
                }
                if (!string.IsNullOrWhiteSpace(updateDto.Title))
                    book.Title = updateDto.Title;

                if (!string.IsNullOrWhiteSpace(updateDto.Annotation))
                    book.Annotation = updateDto.Annotation;

                if (!string.IsNullOrWhiteSpace(updateDto.Image))
                    book.Image = updateDto.Image;

                if (updateDto.PageCount.HasValue)
                    book.PageCount = updateDto.PageCount.Value;
                if (!string.IsNullOrWhiteSpace(updateDto.AuthorName))
                {
                    var author = _repository.Author.GetAuthorByName(updateDto.AuthorName, false);
                    if (author == null)
                    {
                        author = new Author { AuthorName = updateDto.AuthorName };
                        _repository.Author.CreateAuthor(author);
                        _repository.Save();
                    }
                    book.AuthorID = author.Id;
                }
                if (!string.IsNullOrWhiteSpace(updateDto.GenreName))
                {
                    var genre = _repository.Genre.GetGenreByName(updateDto.GenreName, false);
                    if (genre == null)
                    {
                        genre = new Genre { GenreName = updateDto.GenreName };
                        _repository.Genre.CreateGenre(genre);
                        _repository.Save();
                    }
                    book.GenreID = genre.Id;
                }

                bool hasReadingStatusUpdate =
                    updateDto.Rating.HasValue ||
                    !string.IsNullOrWhiteSpace(updateDto.Review) ||
                    !string.IsNullOrWhiteSpace(updateDto.Quotes) ||
                    updateDto.StartReadingDate.HasValue ||
                    updateDto.EndReadingDate.HasValue;

                if (hasReadingStatusUpdate)
                {
                    if (book.ReadingStatus == null)
                    {
                        var readingStatus = new ReadingStatus
                        {
                            Rating = updateDto.Rating.GetValueOrDefault(), // 👈 вот тут фикс
                            Review = updateDto.Review,
                            Quotes = updateDto.Quotes,
                            StartReadingDate = updateDto.StartReadingDate,
                            EndReadingDate = updateDto.EndReadingDate
                        };
                        book.ReadingStatus = readingStatus;
                        _repository.ReadingStatus.CreateReadingStatus(readingStatus);
                    }
                    else
                    {
                        if (updateDto.Rating.HasValue)
                            book.ReadingStatus.Rating = updateDto.Rating.Value;

                        if (!string.IsNullOrWhiteSpace(updateDto.Review))
                            book.ReadingStatus.Review = updateDto.Review;

                        if (!string.IsNullOrWhiteSpace(updateDto.Quotes))
                            book.ReadingStatus.Quotes = updateDto.Quotes;

                        if (updateDto.StartReadingDate.HasValue)
                            book.ReadingStatus.StartReadingDate = updateDto.StartReadingDate;

                        if (updateDto.EndReadingDate.HasValue)
                            book.ReadingStatus.EndReadingDate = updateDto.EndReadingDate;
                    }
                }
                _repository.Save();

                return Ok("Книга успешно обновлена.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обновлении книги: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
 
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id);
                if (book == null)
                    return NotFound("Книга не найдена.");

                _repository.Book.DeleteBook(book);
                _repository.Save();

                return Ok("Книга удалена.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при удалении книги: {ex}");
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
   