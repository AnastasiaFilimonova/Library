using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [Route("api/[controller]")]
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
        [HttpPost]
        public IActionResult CreateGenre([FromBody] GenreDTO genreDto)
        {
            if (string.IsNullOrWhiteSpace(genreDto.GenreName))
                return BadRequest("Название жанра не может быть пустым.");

            var normalizedGenreName = NormalizeName(genreDto.GenreName);

            var existing = _repository.Genre
                .FindByCondition(g => g.GenreName.ToLower().Trim() == normalizedGenreName.ToLower().Trim(), false)
                .FirstOrDefault();

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

        private string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.Trim().ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }



}

