using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/v1/upload")]
    public class FileUploadController : ControllerBase
    {
        private readonly TokenHelper _tokenHelper;
        private readonly ApplicationDbContext _context;
        private const int MaxSize = 16777215; // 16,777,215 байтов

        public FileUploadController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _tokenHelper = tokenHelper; // Помощник для работы с JWT.
            _context = context;
        }
        [HttpPost("file")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string currentToken)
        {
            // Извлечение имени пользователя из токена.
            if (_tokenHelper.IsTokenExpired(currentToken))
                return Unauthorized(new { Message = "Expired token" });

            var currentTokenId = _tokenHelper.GetCurrentTokenId(currentToken);

            // Проверка действительности токена.
            if (_tokenHelper.IsInvalidToken(currentTokenId))
                return Unauthorized(new { Message = "This token has been invalidated." });

            if (file == null || file.Length == 0)
                return BadRequest("Пожалуйста, предоставьте файл для загрузки.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "temp", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            //System.IO.File.Delete(path);
            return Ok($"Файл {file.FileName} успешно загружен.");
        }
    }
}
