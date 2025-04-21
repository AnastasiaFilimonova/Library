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
    [Route("api/[controller]")]
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == userDto.Login);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует." });
            }

            var user = new User
            {
                Login = userDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                UserName = userDto.Login
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.Id, user.Login);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO loginDto)
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

