using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library.Controllers
{
    [Route("api/genres")]
    [ApiController]
    [Authorize]
    public class GenresController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public GenresController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Создание новый жанр книги
        /// </summary>
        /// <param name="genreDto">Данные жанра</param>
        /// <returns>Информация о созданном жанре</returns>
        /// <response code="200">Жанр успешно создан</response>
        /// <response code="400">Название жанра не указано</response>
        /// <response code="409">Такой жанр уже существует</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public IActionResult CreateGenre([FromBody] GenreDTO genreDto)
        {
            if (string.IsNullOrWhiteSpace(genreDto.GenreName))
                return BadRequest("Название жанра не может быть пустым.");
            var normalizedGenreName = genreDto.GenreName.Trim(); 
            var existing = _repository.Genre.FindByCondition(g => g.GenreName.Trim() == normalizedGenreName, false).FirstOrDefault();
            if (existing != null)
                return Conflict("Жанр уже существует");
            try
            {
                var genre = new Genre { GenreName = normalizedGenreName };
                _repository.Genre.CreateGenre(genre);
                _repository.Save();
                return Ok(genre);
            }
            catch (DbUpdateException)
            {
                return Conflict("Жанр уже существует (ограничение уникальности сработало)");
            }
        }
    }
}

