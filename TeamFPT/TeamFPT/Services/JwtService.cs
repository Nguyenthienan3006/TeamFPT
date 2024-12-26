using Microsoft.IdentityModel.Tokens;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.Models;
using Microsoft.AspNetCore.Identity.Data;
using TeamFPT.Models.API;

namespace TeamFPT.Services
{
	public class JwtService
	{
		private readonly IConfiguration _config;
		private readonly ConnectService _connectService;
		private readonly EmailService _emailService;

		public JwtService(ConnectService connectService, IConfiguration configuration, EmailService emailService)
		{
			_connectService = connectService;
			_config = configuration;
			_emailService = emailService;
		}
		public string GenerateToken(User user)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.NameIdentifier, user.Username),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Role, user.Role)
				}),
				Issuer = _config["Jwt:Issuer"],
				Expires = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
				Audience = _config["Jwt:Audience"],
				SigningCredentials = credentials
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var securityToken = tokenHandler.CreateToken(tokenDescriptor);
			var accessToken = tokenHandler.WriteToken(securityToken);

			return accessToken;
		}

	}
}
