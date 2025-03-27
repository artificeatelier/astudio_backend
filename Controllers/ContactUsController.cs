using backend.Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUSController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(ContactUsPostModel value)
        {
            Console.WriteLine("Endpoint reached");

            // Save the data
            ContactUsData.Data.Add(value);

            // Send email
            bool emailSent = await SendEmailAsync(value);

            if (emailSent)
                return Ok(new { message = "Message received, email sent successfully." });
            else
                return StatusCode(500, "Message received, but email sending failed.");
        }

        private async Task<bool> SendEmailAsync(ContactUsPostModel value)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("bhavyavasoya01@gmail.com"));
                email.To.Add(MailboxAddress.Parse("bhavyanvasoya@gmail.com"));
                email.Subject = "Contact Us Form";
                email.Body = new TextPart(TextFormat.Html) { Text = $"<h1>Messages: {value.Message}</h1>" +
                    $"</br>" +
                    $"<h2>Name: {value.Name}</h2>" +
                    $"<h2>Mail-ID: {value.Email}</h2>" +
                    $"<h2>Phone Number: {value.Phone}</h2>" };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("bhavyanvasoya@gmail.com", "jebfielqfqhdhfwx");
                smtp.Send(email);
                smtp.Disconnect(true);

                Console.WriteLine("Email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
