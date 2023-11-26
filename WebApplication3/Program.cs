using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Server.Kestrel;
using System.Net;
using WebApplication3;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args); // Создание и инициализация объекта, который предоставляет средства для создания приложения.
var configuration = builder.Configuration; // Получение объекта конфигурации, который позволяет работать с настройками приложения.

// Конфигурация веб-сервера Kestrel.
builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 5267); // Настройка сервера на прослушивание всех доступных IP-адресов на порту 5267 (HTTP).
    options.Listen(IPAddress.Any, 7084, listenOptions =>
    {
        listenOptions.UseHttps("C:\\Users\\King\\Desktop\\net7.0\\certificate.pfx", "password"); // Настройка сервера на прослушивание всех доступных IP-адресов на порту 7084 (HTTPS).
    });
});

// Добавление контекста базы данных с использованием MySQL в качестве провайдера базы данных.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 33))));

// Добавление и настройка аутентификации на основе JWT-токенов.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Настройка параметров валидации JWT-токена.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Const.SecretKeyValue)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        builder =>
        {
            builder.WithOrigins("*", "*") // Замените эти источники на актуальные для вашего приложения
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


// Добавление сервисов в контейнер зависимостей.
builder.Services.AddControllers();// Добавление контроллеров.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();// Добавление инструмента для исследования API.
builder.Services.AddSwaggerGen(); // Добавление инструментов для генерации документации Swagger.
builder.Services.AddTransient<IEmailSender, EmailSender>(); // Добавление сервиса отправки электронной почты.
builder.Services.AddTransient<TokenHelper>(); // Добавление помощника по работе с токенами.
builder.Services.AddTransient<TokenCleanupService>(); // Добавление сервиса очистки токенов.
builder.Services.AddHostedService<TokenCleanupHostedService>(); // Добавление фонового сервиса для очистки токенов.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Построение приложения на основе ранее заданных настроек.
var app = builder.Build();

// Конфигурация конвейера обработки HTTP-запросов.
if (app.Environment.IsDevelopment()) // Проверка, работает ли приложение в режиме разработки.
{
    app.UseSwagger(); // Включение маршрутизации для документации Swagger.
    app.UseSwaggerUI(); // Включение пользовательского интерфейса Swagger.
}

app.UseRouting();

app.UseCors("MyPolicy");


app.UseAuthentication(); // Затем аутентификация
app.UseAuthorization(); // И после - авторизация

app.MapControllers(); // Настройка маршрутизации для контроллеров.

app.UseHttpsRedirection(); // Включение переадресации с HTTP на HTTPS.
app.UseRequestLogging();


app.Run(); // Запуск приложения.



