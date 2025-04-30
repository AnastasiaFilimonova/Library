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
        public async Task<IActionResult> CreateBook([FromForm] BookDTO bookDTO)
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
            string imagePath = null;
            if (bookDTO.Image != null && bookDTO.Image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bookDTO.Image.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await bookDTO.Image.CopyToAsync(stream);
                }
                imagePath = "/images/" + fileName;
            }
            var book = _mapper.Map<Book>(bookDTO);
            book.AuthorID = author.Id;
            book.GenreID = genre.Id;
            book.Image = imagePath;
            var readingStatus = new ReadingStatus { Status = 0 };
            _repository.ReadingStatus.CreateReadingStatus(readingStatus);
            _repository.Save();
            book.ReadingStatusID = readingStatus.Id;
            book.ReadingStatus = readingStatus;
            _repository.Book.CreateBook(book);
            _repository.Save();
            _repository.ListBook.Create(new ListBook { UserID = userId, BookID = book.Id });
            _repository.Save();
            var fullBook = _repository.Book.GetAllBooks(false).FirstOrDefault(b => b.Id == book.Id && b.ListBooks.Any(lb => lb.UserID == userId));
            if (fullBook == null)
                return BadRequest("Ошибка при получении книги после добавления.");
            var bookDto = _mapper.Map<BookListDTO>(fullBook);
            return Ok(new
            {
                bookDto.Id,
                bookDto.Title,
                bookDto.AuthorName,
                bookDto.Status,
                Message = "Книга успешно добавлена в библиотеку!"
            });
        }

        /// <summary>
        /// Обновление информации о книге. Основные поля (название, жанр, автор, аннотация, количество страниц и обложка) можно изменять всегда. Данные о прочтении (оценка, рецензия, цитаты и даты) — только если установлен статус "прочитана".
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <param name="updateDto">Новые данные книги</param>
        /// <returns>Результат обновления</returns>
        /// <response code="200">Книга успешно обновлена</response>
        /// <response code="400">Некорректные данные или книга не прочитана</response>
        /// <response code="404">Книга не найдена</response>
        [HttpPost("update/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDTO updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id && b.ListBooks.Any(lb => lb.UserID == userId));
                if (book == null)
                    return NotFound("Книга не найдена.");
                if (!string.IsNullOrWhiteSpace(updateDto.Title))
                    book.Title = updateDto.Title.Trim();
                if (!string.IsNullOrWhiteSpace(updateDto.Annotation))
                    book.Annotation = updateDto.Annotation.Trim();
                if (updateDto.PageCount.HasValue)
                    book.PageCount = updateDto.PageCount.Value;
                if (!string.IsNullOrWhiteSpace(updateDto.AuthorName))
                {
                    var author = _repository.Author.GetAuthorByName(updateDto.AuthorName.Trim(), false) ?? new Author { AuthorName = updateDto.AuthorName.Trim() };
                    if (author.Id == 0)
                    {
                        _repository.Author.CreateAuthor(author);
                        _repository.Save();
                    }
                    book.AuthorID = author.Id;
                }
                if (!string.IsNullOrWhiteSpace(updateDto.GenreName))
                {
                    var genre = _repository.Genre.GetGenreByName(updateDto.GenreName.Trim(), false)
                                 ?? new Genre { GenreName = updateDto.GenreName.Trim() };
                    if (genre.Id == 0)
                    {
                        _repository.Genre.CreateGenre(genre);
                        _repository.Save();
                    }
                    book.GenreID = genre.Id;
                }
                if (updateDto.Image != null && updateDto.Image.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(updateDto.Image.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await updateDto.Image.CopyToAsync(stream);
                    }
                    if (!string.IsNullOrEmpty(book.Image))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    book.Image = "/images/" + fileName;
                }
                bool hasReadingStatusUpdate = updateDto.Rating.HasValue || !string.IsNullOrWhiteSpace(updateDto.Review) || !string.IsNullOrWhiteSpace(updateDto.Quotes) || updateDto.StartReadingDate.HasValue || updateDto.EndReadingDate.HasValue;
                if (hasReadingStatusUpdate && updateDto.Status != 1)
                {
                    return BadRequest("Поля, связанные с прочтением книги, можно редактировать только если книга помечена как прочитанная (Status = 1).");
                }
                if (updateDto.Status == 1)
                {
                    if (book.ReadingStatus == null)
                    {
                        var readingStatus = _mapper.Map<ReadingStatus>(updateDto);
                        readingStatus.Status = 1;
                        _repository.ReadingStatus.CreateReadingStatus(readingStatus);
                        _repository.Save();
                        book.ReadingStatus = readingStatus;
                        book.ReadingStatusID = readingStatus.Id;
                    }
                    else
                    {
                        _mapper.Map(updateDto, book.ReadingStatus);
                        book.ReadingStatus.Status = 1;
                    }
                }
                _repository.Save();
                return Ok("Книга успешно обновлена.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при обновлении книги: {ex}");
                return BadRequest("Ошибка при обновлении книги.");
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
            var books = _repository.Book.GetFilteredBooks(bookParams, false).Where(b => b.ListBooks != null && b.ListBooks.Any(lb => lb.UserID == userId));
            var booksDto = _mapper.Map<IEnumerable<BookListDTO>>(books);
            return Ok(booksDto);
        }

    }
}
