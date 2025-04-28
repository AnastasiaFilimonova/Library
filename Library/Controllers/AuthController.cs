using AutoMapper;
using Contracts;
using Entities;
using Entities.DataTransferObject;
using Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog.Config;

namespace Library.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly JwtService _jwtService;
        private readonly RepositoryContext _context;

        public AuthController(IRepositoryManager repository, ILoggerManager logger, JwtService jwtService, RepositoryContext context)
        {
            _repository = repository;
            _logger = logger;
            _jwtService = jwtService;
            _context = context;
        }

        /// <summary>
        /// Регистрирует нового пользователя в системе
        /// </summary>
        /// <param name="userDto">Данные пользователя для регистрации.</param>
        /// <returns>JWT-токен при успешной регистрации.</returns>
        /// <response code="200">Пользователь успешно зарегистрирован.</response>
        /// <response code="400">Пользователь с таким логином уже существует.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == userDto.Login);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует." });
            }
            if (string.IsNullOrWhiteSpace(userDto.Password) || userDto.Password.Length < 6)
            {
                return BadRequest(new { message = "Пароль должен содержать минимум 6 символов." });
            }
            var user = new User
            {
                Login = userDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                UserName = userDto.UserName 
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = _jwtService.GenerateToken(user.Id, user.Login);
            return Ok(new { token });
        }
        /// <summary>
        /// Выполняет вход пользователя в систему
        /// </summary>
        /// <param name="loginDto">Данные пользователя для входа.</param>
        /// <returns>JWT-токен при успешной авторизации.</returns>
        /// <response code="200">Успешный вход в систему.</response>
        /// <response code="401">Неверный логин или пароль.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Неверный логин или пароль." });
            }
            var token = _jwtService.GenerateToken(user.Id, user.Login);
            return Ok(new { token });
        }
    }
}

