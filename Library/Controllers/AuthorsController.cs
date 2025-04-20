using AutoMapper;
using Contracts;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorDTO authorDto)
        {
            if (string.IsNullOrWhiteSpace(authorDto.AuthorName))
                return BadRequest("Имя автора не может быть пустым");

            var normalized = authorDto.AuthorName.Trim().ToLower();

            var existing = _repository.Author
                .FindByCondition(a => a.AuthorName.ToLower().Trim() == normalized, false)
                .FirstOrDefault();

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
            catch (DbUpdateException ex)
            {
                // Это на случай, если всё-таки ошибка дублирования проскользнёт
                return Conflict("Автор уже существует (ограничение уникальности сработало)");
            }
        }



    }
}
