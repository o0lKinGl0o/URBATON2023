using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Server.Kestrel;
using System.Net;
using WebApplication3;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args); // �������� � ������������� �������, ������� ������������� �������� ��� �������� ����������.
var configuration = builder.Configuration; // ��������� ������� ������������, ������� ��������� �������� � ����������� ����������.

// ������������ ���-������� Kestrel.
builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 5267); // ��������� ������� �� ������������� ���� ��������� IP-������� �� ����� 5267 (HTTP).
    options.Listen(IPAddress.Any, 7084, listenOptions =>
    {
        listenOptions.UseHttps("C:\\Users\\King\\Desktop\\net7.0\\certificate.pfx", "password"); // ��������� ������� �� ������������� ���� ��������� IP-������� �� ����� 7084 (HTTPS).
    });
});

// ���������� ��������� ���� ������ � �������������� MySQL � �������� ���������� ���� ������.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 33))));

// ���������� � ��������� �������������� �� ������ JWT-�������.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // ��������� ���������� ��������� JWT-������.
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
            builder.WithOrigins("*", "*") // �������� ��� ��������� �� ���������� ��� ������ ����������
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


// ���������� �������� � ��������� ������������.
builder.Services.AddControllers();// ���������� ������������.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();// ���������� ����������� ��� ������������ API.
builder.Services.AddSwaggerGen(); // ���������� ������������ ��� ��������� ������������ Swagger.
builder.Services.AddTransient<IEmailSender, EmailSender>(); // ���������� ������� �������� ����������� �����.
builder.Services.AddTransient<TokenHelper>(); // ���������� ��������� �� ������ � ��������.
builder.Services.AddTransient<TokenCleanupService>(); // ���������� ������� ������� �������.
builder.Services.AddHostedService<TokenCleanupHostedService>(); // ���������� �������� ������� ��� ������� �������.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// ���������� ���������� �� ������ ����� �������� ��������.
var app = builder.Build();

// ������������ ��������� ��������� HTTP-��������.
if (app.Environment.IsDevelopment()) // ��������, �������� �� ���������� � ������ ����������.
{
    app.UseSwagger(); // ��������� ������������� ��� ������������ Swagger.
    app.UseSwaggerUI(); // ��������� ����������������� ���������� Swagger.
}

app.UseRouting();

app.UseCors("MyPolicy");


app.UseAuthentication(); // ����� ��������������
app.UseAuthorization(); // � ����� - �����������

app.MapControllers(); // ��������� ������������� ��� ������������.

app.UseHttpsRedirection(); // ��������� ������������� � HTTP �� HTTPS.
app.UseRequestLogging();


app.Run(); // ������ ����������.



