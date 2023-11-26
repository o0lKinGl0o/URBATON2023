using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace WebApplication3
{
    public interface IEmailSender
    {
        Task EmailConfirmationMessage(string email, string confirmationCode);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task EmailConfirmationMessage(string email, string confirmationCode)
        {
            var message = new MimeMessage();
            try
            {
                message.From.Add(new MailboxAddress("XakatonApp", _configuration["Email:Username"]));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Подтверждение почты";

                message.Body = new TextPart("plain")
                {
                    Text = $"Подтвердите ваш Email перейдя по ссылке: {confirmationCode}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
            await SendMessage(message);
        }
        private async Task SendMessage(MimeMessage message)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_configuration["Email:SmtpServer"], _configuration.GetValue<int>("Email:Port"), MailKit.Security.SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
            }
        }
    }
}
