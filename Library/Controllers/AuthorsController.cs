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
    [Route("api/authors")]
    [ApiController]
    [Authorize]
    public class AuthorsController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Создание нового автора
        /// </summary>
        /// <param name="authorDto">Данные автора.</param>
        /// <returns>Информация о созданном авторе.</returns>
        /// <response code="200">Автор успешно создан.</response>
        /// <response code="400">Имя автора отсутствует.</response>
        /// <response code="409">Автор с таким именем уже существует.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateAuthor([FromBody] AuthorDTO authorDto)
        {
            if (string.IsNullOrWhiteSpace(authorDto.AuthorName))
                return BadRequest("Имя автора не может быть пустым");

            var normalized = authorDto.AuthorName.Trim();

            var existing = _repository.Author.FindByCondition(a => a.AuthorName.ToLower().Trim() == normalized, false).FirstOrDefault();

            if (existing != null)
                return Conflict("Автор уже существует");
            try
            {
                var author = new Author
                {
                    AuthorName = char.ToUpper(normalized[0]) + normalized.Substring(1)
                };
                _repository.Author.CreateAuthor(author);
                _repository.Save();
                return Ok(author);
            }
            catch (DbUpdateException)
            {
                return Conflict("Автор уже существует");
            }
        }
    }
}


