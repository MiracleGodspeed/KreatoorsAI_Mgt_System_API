using KreatoorsAI.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Core.Services
{
#pragma warning disable
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(recipientEmail), "to");
                content.Add(new StringContent($"KreatoorsAI <noreply@kobokistltd.com>"), "from");
                content.Add(new StringContent(subject), "subject");
                content.Add(new StringContent(body), "html");

                _httpClient.BaseAddress = new Uri($"{_configuration["Mailgun:Url"]}");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"api:{_configuration["Mailgun:Key"]}")));

                var response = await _httpClient.PostAsync("messages", content);
                response.EnsureSuccessStatusCode();
            }
        }
    }

}
