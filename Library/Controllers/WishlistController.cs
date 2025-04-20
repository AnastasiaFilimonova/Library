using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Entities.Models;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpGet]
        public IActionResult GetWishlist()
        {
            try
            {
                var wishlist = _repository.Wishlist.GetAllWishlistItems(false);
                var wishlistDto = _mapper.Map<IEnumerable<WishlistDTO>>(wishlist);
                return Ok(wishlistDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в GetWishlist: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost]
        public IActionResult AddToWishlist([FromBody] NewWishlistDTO dto)
        {
            try
            {
                var author = _repository.Author.GetAuthorByName(dto.AuthorName.Trim(), false)
                              ?? new Author { AuthorName = dto.AuthorName.Trim() };

                if (author.Id == 0)
                {
                    _repository.Author.CreateAuthor(author);
                    _repository.Save();
                }

                var genre = _repository.Genre.GetGenreByName(dto.GenreName.Trim(), false)
                              ?? new Genre { GenreName = dto.GenreName.Trim() };

                if (genre.Id == 0)
                {
                    _repository.Genre.CreateGenre(genre);
                    _repository.Save();
                }

                var book = _mapper.Map<Book>(dto);
                book.AuthorID = author.Id;
                book.GenreID = genre.Id;

                _repository.Book.CreateBook(book);
                _repository.Save();

                var wishlistItem = new Wishlist
                {
                    BookID = book.Id,
                    UserID = 1 // если у тебя пока нет авторизации
                };

                _repository.Wishlist.AddToWishlist(wishlistItem);
                _repository.Save();

                return Ok(new { Message = "Книга добавлена в список желаний." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в AddToWishlist: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }



        [HttpDelete("{id}")]
        public IActionResult RemoveFromWishlist(int id)
        {
            try
            {
                var item = _repository.Wishlist.GetWishlistItem(id, false);
                if (item == null)
                    return NotFound("Книга не найдена в списке желаний.");

                _repository.Wishlist.RemoveFromWishlist(item);
                _repository.Save();
                return Ok("Книга удалена из списка желаний.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RemoveFromWishlist: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("purchase/{id}")]
        public IActionResult PurchaseBook(int id)
        {
            try
            {
                var item = _repository.Wishlist.GetWishlistItem(id, true);
                if (item == null)
                    return NotFound("Книга не найдена в списке желаний.");

                var book = item.Book;

                var readingStatus = new ReadingStatus
                {
                    Status = (int)ReadingStatusEnum.NotRead
                };

                _repository.ReadingStatus.CreateReadingStatus(readingStatus);
                _repository.Save();

                book.ReadingStatusID = readingStatus.Id;
                book.ReadingStatus = readingStatus;
                _repository.Book.UpdateBook(book);

                _repository.Wishlist.RemoveFromWishlist(item);
                _repository.Save();

                return Ok("Книга куплена и добавлена в библиотеку.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в PurchaseBook: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}

