using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Controllers
{
    // Определение маршрута и атрибута контроллера для данного класса контроллера.
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GetController : Controller
    {
        // Объявляются частные поля для контекста базы данных и вспомогательного класса для работы с токенами.
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;

        // Конструктор контроллера с внедрением зависимостей для контекста базы данных и вспомогательного класса токенов.
        public GetController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }

        // Метод действия контроллера для получения имени пользователя из токена.
        [HttpPost("username-from-token")]
        public IActionResult GetUserNameFromToken([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            try
            {
                // Извлечение имени пользователя из токена.
                if (_tokenHelper.IsTokenExpired(authorizationHeader))
                    return Unauthorized(new { Message = "Expired token" });

                // Получение идентификатора текущего токена.
                var currentTokenId = _tokenHelper.GetCurrentTokenId(authorizationHeader);

                // Проверка действительности токена.
                if (_tokenHelper.IsInvalidToken(currentTokenId))
                    return Unauthorized(new { Message = "This token has been invalidated." });

                // Извлечение имени пользователя из токена.
                var username = _tokenHelper.ExtractUsernameFromToken(Request.Headers["Authorization"].ToString());

                // Если имя пользователя найдено, возвращается ответ с именем пользователя.
                if (username != null)
                    return Ok(new { UserName = username });
                // В противном случае возвращается ответ об ошибке.
                else
                    return Unauthorized(new { Message = "Invalid token" });
            }
            // Обработка исключений и возврат ошибки сервера.
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
        [HttpGet("posts")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.post.ToListAsync();
        }
    }
}
