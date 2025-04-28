using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Entities.Models;
using Entities.RequestFeatures;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;

namespace Library.Controllers
{
    [Route("api/books")]
    [ApiController]
    [Authorize]
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
        /// <summary>
        /// Возвращает список всех книг пользователя
        /// </summary>
        /// <returns>Список книг</returns>
        /// <response code="200">Успешно возвращён список книг</response>
        /// <response code="400">Ошибка при получении данных</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetBooks()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var books = _repository.Book.GetAllBooks(false).Where(b => b.ListBooks.Any(lb => lb.UserID == userId));
                var booksDto = _mapper.Map<IEnumerable<BookListDTO>>(books);
                return Ok(booksDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в GetBooks: {ex}");
                return BadRequest("Ошибка при получении списка книг");
            }
        }

        /// <summary>
        /// Возвращает подробную информацию о книге по ID
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Детали книги</returns>
        /// <response code="200">Книга найдена</response>
        /// <response code="404">Книга не найдена</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetBookById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var book = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Id == id && b.ListBooks.Any(lb => lb.UserID == userId));
            if (book == null) return NotFound("Книга не найдена.");
            var dto = _mapper.Map<BookDetailsDTO>(book);
            return Ok(dto);
        }

        /// <summary>
        /// Добавление книги в библиотеку пользователя
        /// </summary>
        /// <param name="bookDTO">Данные книги</param>
        /// <returns>Результат добавления</returns>
        /// <response code="200">Книга успешно добавлена или уже существует</response>
        /// <response code="400">Некорректные данные</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult CreateBook([FromBody] BookDTO bookDTO)
        {
            if (bookDTO == null)
                return BadRequest("Некорректные данные книги.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var author = _repository.Author.GetAuthorByName(bookDTO.AuthorName, false);
            var genre = _repository.Genre.GetGenreByName(bookDTO.GenreName, false);
            if (author == null || genre == null)
                return BadRequest("Автор или жанр не найдены. Добавьте их сначала.");
            var normalizedTitle = bookDTO.Title?.Trim().ToLower();
            var existingBook = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Title.ToLower().Trim() == normalizedTitle && b.AuthorID == author.Id);
            if (existingBook != null)
            {
                if (!_repository.ListBook.Exists(userId, existingBook.Id))
                {
                    _repository.ListBook.Create(new ListBook { UserID = userId, BookID = existingBook.Id });
                    _repository.Save();
                }
                return Ok(new { existingBook.Id, Status = "Уже добавлена ранее" });
            }
            var book = _mapper.Map<Book>(bookDTO);
            book.AuthorID = author.Id;
            book.GenreID = genre.Id;
            _repository.Book.CreateBook(book);
            _repository.Save();
            _repository.ListBook.Create(new ListBook { UserID = userId, BookID = book.Id });
            _repository.Save();
            return Ok(new { book.Id, Status = "Не прочитана", Message = "Книга успешно добавлена в библиотеку!" });
        }

        /// <summary>
        /// Обновление информации о прочитанной книге
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <param name="updateDto">Новые данные</param>
        /// <returns>Результат обновления</returns>
        /// <response code="200">Книга успешно обновлена</response>
        /// <response code="400">Редактирование доступно только для прочитанных книг</response>
        /// <response code="404">Книга не найдена</response>
        [HttpPost("update/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateBook(int id, [FromBody] BookUpdateDTO updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var book = _repository.Book.GetAllBooks(true)
                    .FirstOrDefault(b => b.Id == id && b.ListBooks.Any(lb => lb.UserID == userId));
                if (book == null)
                    return NotFound("Книга не найдена.");
                if (updateDto.Status != 1)
                    return BadRequest("Редактирование доступно только для прочитанных книг.");
                _mapper.Map(updateDto, book);
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
                bool hasReadingStatusUpdate = updateDto.Rating.HasValue || !string.IsNullOrWhiteSpace(updateDto.Review) | !string.IsNullOrWhiteSpace(updateDto.Quotes) || updateDto.StartReadingDate.HasValue || updateDto.EndReadingDate.HasValue || updateDto.Status.HasValue;
                if (hasReadingStatusUpdate)
                {
                    if (book.ReadingStatus == null)
                    {
                        var readingStatus = _mapper.Map<ReadingStatus>(updateDto);
                        readingStatus.Status = updateDto.Status ?? 1;
                        _repository.ReadingStatus.CreateReadingStatus(readingStatus);
                        _repository.Save();
                        book.ReadingStatus = readingStatus;
                        book.ReadingStatusID = readingStatus.Id;
                    }
                    else
                    {
                        _mapper.Map(updateDto, book.ReadingStatus);
                        if (updateDto.Status.HasValue)
                            book.ReadingStatus.Status = updateDto.Status.Value;
                    }
                }
                _repository.Save();
                return Ok("Книга успешно обновлена.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обновлении книги: {ex}");
                return BadRequest();
            }
        }
        /// <summary>
        /// Удаление книги из библиотеки пользователя
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Результат удаления</returns>
        /// <response code="200">Книга удалена</response>
        /// <response code="400">Книга удалена</response>
        /// <response code="404">Книга не найдена</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id && b.ListBooks.Any(lb => lb.UserID == userId));
                if (book == null)
                    return NotFound("Книга не найдена.");
                _repository.Book.DeleteBook(book);
                _repository.Save();
                return Ok("Книга удалена.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при удалении книги: {ex}");
                return BadRequest("Ошибка при удалении книги");
            }
        }
        /// <summary>
        /// Импорт книги из Google Books API
        /// </summary>
        /// <param name="bookDto">Данные книги</param>
        /// <returns>Результат импорта</returns>
        /// <response code="200">Книга успешно импортирована или уже существует</response>
        [HttpPost("import")]
        [ProducesResponseType(200)]
        public IActionResult ImportBook([FromBody] GoogleBookDTO bookDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var author = _repository.Author.GetAuthorByName(bookDto.AuthorName, false);
            if (author == null)
            {
                author = new Author { AuthorName = bookDto.AuthorName };
                _repository.Author.CreateAuthor(author);
                _repository.Save();
            }
            var genre = _repository.Genre.GetGenreByName(bookDto.GenreName, false);
            if (genre == null)
            {
                genre = new Genre { GenreName = bookDto.GenreName };
                _repository.Genre.CreateGenre(genre);
                _repository.Save();
            }
            var normalizedTitle = bookDto.Title?.Trim().ToLower();
            var existingBook = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Title.ToLower().Trim() == normalizedTitle && b.AuthorID == author.Id);
            if (existingBook != null)
            {
                if (!_repository.ListBook.Exists(userId, existingBook.Id))
                {
                    _repository.ListBook.Create(new ListBook { UserID = userId, BookID = existingBook.Id });
                    _repository.Save();
                }
                return Ok(new { Message = "Книга уже существует и добавлена в вашу библиотеку.", existingBook.Id });
            }
            var book = new Book
            {
                Title = bookDto.Title,
                AuthorID = author.Id,
                GenreID = genre.Id,
                Annotation = bookDto.Annotation,
                PageCount = bookDto.PageCount,
                Image = bookDto.Image
            };
            _repository.Book.CreateBook(book);
            _repository.Save();
            _repository.ListBook.Create(new ListBook { UserID = userId, BookID = book.Id });
            _repository.Save();
            return Ok(new { Message = "Книга импортирована и добавлена в библиотеку.", book.Id });
        }

        /// <summary>
        /// Фильтрация книги по заданным параметрам
        /// </summary>
        /// <param name="bookParams">Параметры фильтрации</param>
        /// <returns>Список отфильтрованных книг</returns>
        /// <response code="200">Фильтрация прошла успешно</response>
        [HttpGet("filter")]
        [ProducesResponseType(200)]
        public IActionResult FilterBooks([FromQuery] BookParameters bookParams)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var books = _repository.Book.GetFilteredBooks(bookParams, false).Where(b => b.ListBooks.Any(lb => lb.UserID == userId));
            var booksDto = _mapper.Map<IEnumerable<BookListDTO>>(books);
            return Ok(booksDto);
        }
    }
}
