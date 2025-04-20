using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Entities.Models;
using Entities.RequestFeatures;
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
                var booksDto = _mapper.Map<IEnumerable<BookListDTO>>(books);
                return Ok(booksDto);
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

            var dto = _mapper.Map<BookDetailsDTO>(book);
            return Ok(dto);
        }


        [HttpPost]
        public IActionResult CreateBook([FromBody] BookDTO bookDTO)
        {
            if (bookDTO == null)
                return BadRequest("Некорректные данные книги.");

            // 🔍 Используем уже реализованные методы, которые нормализуют строки и ищут в БД
            var author = _repository.Author.GetAuthorByName(bookDTO.AuthorName, false);
            var genre = _repository.Genre.GetGenreByName(bookDTO.GenreName, false);

            if (author == null || genre == null)
                return BadRequest("Автор или жанр не найдены. Добавьте их сначала.");

            // 🔁 Проверяем наличие книги по названию и автору
            var normalizedTitle = bookDTO.Title?.Trim().ToLower();
            var existingBook = _repository.Book.GetAllBooks(false)
                .FirstOrDefault(b =>
                    b.Title.ToLower().Trim() == normalizedTitle &&
                    b.AuthorID == author.Id);

            if (existingBook != null)
                return Conflict("Такая книга уже существует");

            // 🧱 Маппим DTO → сущность
            var book = _mapper.Map<Book>(bookDTO);
            book.AuthorID = author.Id;
            book.GenreID = genre.Id;

            _repository.Book.CreateBook(book);
            _repository.Save();

            return Ok(new
            {
                book.Id,
                Status = "Не прочитана",
                Message = "Книга успешно добавлена в библиотеку!"
            });

        }





        [HttpPost("update/{id}")]
        public IActionResult UpdateBook(int id, [FromBody] BookUpdateDTO updateDto)
        {
            try
            {
                var book = _repository.Book.GetAllBooks(true).FirstOrDefault(b => b.Id == id);
                if (book == null)
                    return NotFound("Книга не найдена.");

                // Проверка, что книга прочитана (enum используется напрямую)
                if (updateDto.Status != 1)
                {
                    return BadRequest("Редактирование доступно только для прочитанных книг.");
                }


                // Обновляем только переданные поля
                _mapper.Map(updateDto, book);
                updateDto.AuthorName = updateDto.AuthorName?.Trim();
               
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
                updateDto.GenreName = updateDto.GenreName?.Trim();
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

                // Проверка, есть ли смысл создавать/обновлять ReadingStatus
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
                        var readingStatus = _mapper.Map<ReadingStatus>(updateDto);
                        readingStatus.Status = updateDto.Status ?? 1; // устанавливаем статус явно
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



        [HttpGet("filter")]
        public IActionResult FilterBooks([FromQuery] BookParameters bookParams)
        {
            var books = _repository.Book.GetFilteredBooks(bookParams, trackChanges: false);
            var booksDto = _mapper.Map<IEnumerable<BookListDTO>>(books);

            // Получаем метаданные и сериализуем
            var paginationMetaData = new
            {
                books.PageNumber,
                books.PageSize,
                books.TotalItemCount,
                books.PageCount,
                books.HasNextPage,
                books.HasPreviousPage
            };
            return Ok(booksDto);
        }



    }
}
   