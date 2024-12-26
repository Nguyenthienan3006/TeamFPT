using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;

namespace TeamFPT.Services

{
	public class EmailService
	{
		public string GenerateOtp()
		{
			const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var randomBytes = new byte[6];
			new RNGCryptoServiceProvider().GetBytes(randomBytes);

			var otp = new string(Enumerable.Range(0, 6)
				.Select(i => validChars[randomBytes[i] % validChars.Length])
				.ToArray());

			return new string(otp);
		}


		public string SendOtpEmail(string recipientEmail)
		{
			string smtpHost = "smtp.gmail.com"; //SMTP host for gmail
			int smtpPort = 587; 
			string senderEmail = "ngoquochuyvn2004@gmail.com"; // replace with the sender's email
			string senderPassword = "qswq nqke npjw kkwo"; // replace with the sender's password

			string otp = GenerateOtp();
			string subject = "Your OTP Code";
			string body = $"Your One-Time Password (OTP) is: {otp}";

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("OTP Service", senderEmail));
			message.To.Add(new MailboxAddress("", recipientEmail));
			message.Subject = subject;

			var bodyBuilder = new BodyBuilder
			{
				TextBody = body
			};
			message.Body = bodyBuilder.ToMessageBody();

			
				using (var client = new SmtpClient())
				{
				// Connect to SMTP server
				client.Connect(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

				// Authenticate using the sender email credentials (use App Password if 2FA is enabled)
				client.Authenticate(senderEmail, senderPassword);

				// Send the email
				client.Send(message);
				client.Disconnect(true);
				client.Dispose();
			}
			return otp;
		}
			
		}

	}



