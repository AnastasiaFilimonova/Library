using AutoMapper;
using Entities.DataTransferObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Library.Controllers
{
    [Route("api/googlebooks")]
    [ApiController]
    [Authorize]
    public class GoogleBooksController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        public GoogleBooksController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// Поиск книг через Google Books API 
        /// </summary>
        /// <param name="query">Ключевое слово для поиска</param>
        /// <returns>Список книг, найденных в Google Books</returns>
        /// <response code="200">Успешный возврат списка книг</response>
        /// <response code="400">Пустой или некорректный поисковый запрос</response>
        /// <response code="500">Ошибка при обращении к Google Books API</response>
        [HttpGet("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Введите поисковый запрос");
            var apiKey = _config["GoogleBooks:ApiKey"];
            var url = $"https://www.googleapis.com/books/v1/volumes?q={query}&key={apiKey}";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Ошибка при запросе к Google Books API");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleBooksResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var books = result?.Items?.Select(item => new GoogleBookDTO
            {
                Title = item.VolumeInfo?.Title,
                AuthorName = item.VolumeInfo?.Authors?.FirstOrDefault() ?? "Неизвестно",
                GenreName = item.VolumeInfo?.Categories?.FirstOrDefault() ?? "Неизвестно",
                Annotation = item.VolumeInfo?.Description,
                PageCount = item.VolumeInfo?.PageCount ?? 0,
                Image = item.VolumeInfo?.ImageLinks?.Thumbnail
            }).ToList();
            return Ok(books);
        }
    }
}

