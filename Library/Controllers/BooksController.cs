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
            var books = _repository.Book.GetAllBooks(false);
            var result = _mapper.Map<IEnumerable<BookListDTO>>(books);
            return Ok(result);
        }

        // GET book by id: full info
        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound("Книга не найдена.");

            var dto = _mapper.Map<BookDetailsDTO>(book);
            return Ok(dto);
        }

        // POST: Add a book
        [HttpPost]
        public IActionResult CreateBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                if (bookDTO == null)
                    return BadRequest("Данные книги отсутствуют.");

                // 1. Получаем или создаём автора
                var authorName = bookDTO.AuthorName?.Trim();
                var author = _repository.Author.GetAuthorByName(authorName, false);
                if (author == null)
                {
                    author = new Author { AuthorName = authorName };
                    _repository.Author.CreateAuthor(author);
                    _repository.Save(); // теперь author.Id доступен
                }

                // 2. Получаем или создаём жанр
                var genreName = bookDTO.GenreName?.Trim();
                var genre = _repository.Genre.GetGenreByName(genreName, false);
                if (genre == null)
                {
                    genre = new Genre { GenreName = genreName };
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save(); // теперь genre.Id доступен
                }

                // 3. Проверка на наличие дубликата по Title + Author
                var existingBook = _repository.Book
                    .FindByCondition(b => b.Title.Trim().ToLower() == bookDTO.Title.Trim().ToLower() && b.AuthorID == author.Id, false)
                    .FirstOrDefault();

                if (existingBook != null)
                    return Conflict("Такая книга уже есть в вашей библиотеке.");

                // 4. Маппинг и создание книги
                var book = _mapper.Map<Book>(bookDTO);
                book.AuthorID = author.Id;
                book.GenreID = genre.Id;
                book.ReadingStatusID = null;

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
                _logger.LogError($"Ошибка в CreateBook: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }




        // POST: Update book info
        [HttpPost("update/{id}")]
        public IActionResult UpdateBook(int id, [FromBody] BookUpdateDTO updateDto)
        {
            var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound("Книга не найдена.");

            // Обновление автора
            if (!string.IsNullOrWhiteSpace(updateDto.AuthorName))
            {
                var author = _repository.Author.GetAuthorByName(updateDto.AuthorName.Trim(), false);
                if (author == null)
                {
                    author = new Author { AuthorName = updateDto.AuthorName.Trim() };
                    _repository.Author.CreateAuthor(author);
                    _repository.Save();
                }
                book.AuthorID = author.Id;
            }

            // Обновление жанра
            if (!string.IsNullOrWhiteSpace(updateDto.GenreName))
            {
                var genre = _repository.Genre.GetGenreByName(updateDto.GenreName.Trim(), false);
                if (genre == null)
                {
                    genre = new Genre { GenreName = updateDto.GenreName.Trim() };
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save();
                }
                book.GenreID = genre.Id;
            }

            // Обновление обычных полей книги
            if (updateDto.Title != null) book.Title = updateDto.Title;
            if (updateDto.PageCount.HasValue) book.PageCount = updateDto.PageCount.Value;
            if (updateDto.Annotation != null) book.Annotation = updateDto.Annotation;
            if (updateDto.Image != null) book.Image = updateDto.Image;

            // Если статус == 1 (прочитана), то можно обновлять ReadingStatus
            if (updateDto.Status == 1)
            {
                if (book.ReadingStatus == null)
                {
                    var reading = _mapper.Map<ReadingStatus>(updateDto);
                    reading.Status = 1;
                    _repository.ReadingStatus.CreateReadingStatus(reading);
                    _repository.Save();

                    book.ReadingStatusID = reading.Id;
                    book.ReadingStatus = reading;
                }
                else
                {
                    var rs = book.ReadingStatus;

                    if (updateDto.Rating.HasValue) rs.Rating = updateDto.Rating.Value;
                    if (!string.IsNullOrEmpty(updateDto.Review)) rs.Review = updateDto.Review;
                    if (!string.IsNullOrEmpty(updateDto.Quotes)) rs.Quotes = updateDto.Quotes;
                    if (updateDto.StartReadingDate.HasValue) rs.StartReadingDate = updateDto.StartReadingDate.Value;
                    if (updateDto.EndReadingDate.HasValue) rs.EndReadingDate = updateDto.EndReadingDate.Value;

                    rs.Status = 1;
                }
            }

            _repository.Save();

            // Возврат полной информации
            var updatedDto = _mapper.Map<BookDetailsDTO>(book);
            return Ok(updatedDto);
        }


        // DELETE book
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound("Книга не найдена.");

            _repository.Book.DeleteBook(book);
            _repository.Save();

            return Ok("Книга удалена.");
        }
    }
}

   