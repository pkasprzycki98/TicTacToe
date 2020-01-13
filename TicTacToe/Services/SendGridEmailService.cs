using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Options;

namespace TicTacToe.Services
{
	public class SendGridEmailService : IEmailService
	{
		private EmailServiceOptions _emailServiceOptions;
		private ILogger<EmailService> _logger;
		public SendGridEmailService(IOptions<EmailServiceOptions> emailServiceOptions, ILogger<EmailService> logger)
		{
			_emailServiceOptions = emailServiceOptions.Value;
			_logger = logger;
		}

		public Task SendEmail(string emailTo, string subject, string message)
		{
			_logger.LogInformation($"##Start## Wysyłanie wiadomości e-mail za pomocą usługi SendGrid do:{emailTo} temat:{subject} wiadomość:{message}");
			var client = new SendGrid.SendGridClient(_emailServiceOptions.RemoteServerAPI);
			var sendGridMessage = new SendGrid.Helpers.Mail.SendGridMessage
			{
				From = new SendGrid.Helpers.Mail.EmailAddress(_emailServiceOptions.UserId)
			};
			sendGridMessage.AddTo(emailTo);
			sendGridMessage.Subject = subject;
			sendGridMessage.HtmlContent = message;
			client.SendEmailAsync(sendGridMessage); // wysłanie mail'a
			return Task.CompletedTask;
		}
	}
}
