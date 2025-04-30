using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Entities.Models;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.Controllers
{
    [Route("api/wishlist")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public WishlistController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Возвращает список книг из списка желаний пользователя
        /// </summary>
        /// <returns>Список желаемых книг</returns>
        /// <response code="200">Успешно возвращает список</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult GetWishlist()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var wishlist = _repository.Wishlist.GetAllWishlistItems(false).Where(w => w.UserID == userId);
                var wishlistDto = _mapper.Map<IEnumerable<WishlistDTO>>(wishlist);
                return Ok(wishlistDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в GetWishlist: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        /// <summary>
        /// Добавление книгу в список желаний пользователя
        /// </summary>
        /// <param name="dto">Информация о книге (название, автор, жанр)</param>
        /// <returns>Сообщение об успешном добавлении</returns>
        /// <response code="200">Книга успешно добавлена</response>
        /// <response code="400">Ошибка при добавлении книги</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult AddToWishlist([FromBody] NewWishlistDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var authorName = dto.AuthorName.Trim();
                var genreName = dto.GenreName.Trim();
                var title = dto.Title.Trim();
                var author = _repository.Author.GetAuthorByName(authorName, false) ?? new Author { AuthorName = authorName };
                if (author.Id == 0)
                {
                    _repository.Author.CreateAuthor(author);
                    _repository.Save();
                }
                var genre = _repository.Genre.GetGenreByName(genreName, false) ?? new Genre { GenreName = genreName };
                if (genre.Id == 0)
                {
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save();
                }
                var existingWishlistItem = _repository.Wishlist.GetAllWishlistItems(true).FirstOrDefault(w => w.UserID == userId && w.Book.Title.Trim().ToLower() == title.ToLower() && w.Book.AuthorID == author.Id);
                if (existingWishlistItem != null)
                {
                    return BadRequest("Такая книга уже есть в вашем списке желаний.");
                }
                var book = _mapper.Map<Book>(dto);
                book.AuthorID = author.Id;
                book.GenreID = genre.Id;
                _repository.Book.CreateBook(book);
                _repository.Save();
                var wishlistItem = new Wishlist
                {
                    BookID = book.Id,
                    UserID = userId
                };
                _repository.Wishlist.AddToWishlist(wishlistItem);
                _repository.Save();
                return Ok(new { Message = "Книга добавлена в список желаний." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в AddToWishlist: {ex}");
                return BadRequest("Ошибка при добавлении книги в список желаний");
            }
        }

        /// <summary>
        /// Удаление книги из списка желаний пользователя
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Сообщение об успешном удалении</returns>
        /// <response code="200">Книга удалена</response>
        /// <response code="400">Ошибка при удалении книги</response>
        /// <response code="404">Книга не найдена</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult RemoveFromWishlist(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var item = _repository.Wishlist.GetAllWishlistItems(true).FirstOrDefault(w => w.BookID == id && w.UserID == userId);
                if (item == null)
                    return NotFound("Книга не найдена в списке желаний.");
                _repository.Wishlist.RemoveFromWishlist(item);
                _repository.Save();
                return Ok("Книга удалена из списка желаний.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RemoveFromWishlist: {ex}");
                return BadRequest("Ошибка при удалении книги");
            }
        }
        /// <summary>
        /// Перемещение книги из списка желаний в библиотеку 
        /// </summary>
        /// <param name="id">ID книги</param>
        /// <returns>Сообщение об успешной покупке</returns>
        /// <response code="200">Книга добавлена в библиотеку</response>
        /// <response code="400">Ошибка при получении данных</response>
        /// <response code="404">Книга не найдена в списке желаний</response>
        [HttpPost("purchase/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult PurchaseBook(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var item = _repository.Wishlist.GetAllWishlistItems(true).FirstOrDefault(w => w.BookID == id && w.UserID == userId);
                if (item == null || item.Book == null)
                    return NotFound("Книга не найдена в списке желаний.");
                var book = item.Book;
                if (book.ReadingStatus == null)
                {
                    var readingStatus = new ReadingStatus
                    {
                        Status = (int)ReadingStatusEnum.NotRead
                    };
                    _repository.ReadingStatus.CreateReadingStatus(readingStatus);
                    _repository.Save();
                    book.ReadingStatusID = readingStatus.Id;
                    book.ReadingStatus = readingStatus;
                }
                _repository.Book.UpdateBook(book);
                _repository.Wishlist.RemoveFromWishlist(item);
                if (!_repository.ListBook.Exists(userId, book.Id))
                {
                    _repository.ListBook.Create(new ListBook
                    {
                        UserID = userId,
                        BookID = book.Id
                    });
                }
                _repository.Save();
                return Ok("Книга куплена и добавлена в библиотеку.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в PurchaseBook: {ex}");
                return BadRequest("Ошибка при перемещении книги в библиотеку");
            }
        }
    }
}

