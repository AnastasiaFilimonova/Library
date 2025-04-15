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
                    _logger.LogError("BookDTO object is null.");
                    return BadRequest("Book data is null.");
                }

                // Проверка/создание автора
                var author = _repository.Author.GetAuthorByName(bookDTO.AuthorName, trackChanges: false);
                if (author == null)
                {
                    author = new Author { AuthorName = bookDTO.AuthorName };
                    _repository.Author.CreateAuthor(author);
                    _repository.Save();
                }

                // Проверка/создание жанра
                var genre = _repository.Genre.GetGenreByName(bookDTO.GenreName, trackChanges: false);
                if (genre == null)
                {
                    genre = new Genre { GenreName = bookDTO.GenreName };
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save();
                }

                // 🔍 Проверка на дубликат книги
                var existingBook = _repository.Book.FindByCondition(b => b.Title == bookDTO.Title && b.AuthorID == author.Id, trackChanges: false).FirstOrDefault();

                if (existingBook != null)
                {
                    return Conflict("Такая книга уже есть в вашей библиотеке!.");
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

                return StatusCode(201, "Книга успешно добавлена в библиотеку!");
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

                if (updateDto.ReadingStatusID != null)
                {
                    book.ReadingStatusID = updateDto.ReadingStatusID;
                }

                if (updateDto.ReadingStatusID == (int)ReadingStatusEnum.Read)
                {
                    var readingStatus = book.ReadingStatus ?? new ReadingStatus();

                    if (updateDto.Rating.HasValue)
                        readingStatus.Rating = updateDto.Rating.Value;

                    if (!string.IsNullOrWhiteSpace(updateDto.Review))
                        readingStatus.Review = updateDto.Review;

                    if (!string.IsNullOrWhiteSpace(updateDto.Quotes))
                        readingStatus.Quotes = updateDto.Quotes;

                    if (updateDto.StartReadingDate.HasValue)
                        readingStatus.StartReadingDate = updateDto.StartReadingDate;

                    if (updateDto.EndReadingDate.HasValue)
                        readingStatus.EndReadingDate = updateDto.EndReadingDate;

                    book.ReadingStatus = readingStatus;
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
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
            }
            var employeeForCompany = _repository.Employee.GetEmployee(companyId, id,
            trackChanges: false);
            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            return NotFound();
            }
            _repository.Employee.DeleteEmployee(employeeForCompany);
            _repository.Save();
            return NoContent();
        }


    }
}
   