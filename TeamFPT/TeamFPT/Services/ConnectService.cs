using Microsoft.AspNetCore.Mvc;
using System.Data;
using TeamFPT.Models;
using TeamFPT.Models.API;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace TeamFPT.Services
{
	public class ConnectService
	{
		private readonly string _connectionString;
		public ConnectService(IConfiguration config)
		{
			_connectionString = config.GetConnectionString("DefaultConnection");
		}
		public User Authenticate(LoginRequestModel userLogin)
		{
			User currentUser = null;
			string hashedPassword = GetHashedPassword(userLogin.UserName);
			if (BCrypt.Net.BCrypt.Verify(userLogin.PassWord, hashedPassword))
			{
				using (var connection = new MySqlConnection(_connectionString))
				{
					connection.Open();
					using (var command = new MySqlCommand("Login", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("inputname", userLogin.UserName);
						command.Parameters.AddWithValue("inputpass", hashedPassword);

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								currentUser = new User
								{
									Id = reader.GetInt32("id"),
									Username = reader.GetString("name"),
									Email = reader.GetString("email"),
									Address = reader.GetString("address"),
									Phone = reader.GetString("phone"),
									Role = reader.GetString("role"),
									IsValid = reader.GetBoolean("isvalid")
								};

							}
						}
					}
				}
			}
			return currentUser;
		}


		public bool IsEmailExisted(ResetPassRequestModel model)
		{
			bool userExists = false;

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("CheckEmailofResetRequest", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", model.Username);
					command.Parameters.AddWithValue("inputemail", model.Email);  

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							userExists = true;  
						}
					}
				}
			}

			return userExists; 
		}

		public List<User> GetAllUsers()
		{
			var users = new List<User>();

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("GetAllUsers", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var user = new User
							{
								Id = reader.GetInt32("id"),
								Username = reader.GetString("name"),
								Email = reader.GetString("email"),
								Role = reader.GetString("role"),
								
							};
							users.Add(user);
						}
					}
				}
			}

			return users;
		}

		public List<string> GetAllUserNames()
		{
			var users = new List<string>();

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("GetUserNames", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string username = reader.GetString("name");
							users.Add(username);
						}
					}
				}
			}

			return users;
		}

		public List<string> CHeckEmail()
		{
			var strings = new List<string>();

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("GetEmails", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string email = reader.GetString("email");
							strings.Add(email);
						}
					}
				}
			}

			return strings;
		}
		public void RegisterUser(RegisterRequestModel model, string otp)
		{
			
			string hashedPassword = HashPassword(model.Password);

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("Register", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("registername", model.Username);
					command.Parameters.AddWithValue("registerpassword", hashedPassword); 
					command.Parameters.AddWithValue("registeremail", model.Email);
					command.Parameters.AddWithValue("registeraddress", model.Address);
					command.Parameters.AddWithValue("registerphone", model.Phone);
					command.Parameters.AddWithValue("OTPvalue", otp);
					command.Parameters.AddWithValue("inputdate", DateTime.UtcNow);

					var result = command.ExecuteNonQuery();
				}
			}
		}

		public string HashPassword(string password)
		{
			// Mã hóa mật khẩu sử dụng bcrypt với salt tự động
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
		public string GetHashedPassword(string username)
		{
			string hashedPassword = null;

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("GetHash", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", username);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							hashedPassword = reader.GetString(reader.GetOrdinal("password"));
						}
					}
				}
			}

			return hashedPassword; 
		}

		public void VerifyUser(string username)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("VerifyUser", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", username);
					var result = command.ExecuteNonQuery();
				}
			}

		}
		public void ResetPassword(string username,string password)
		{
			string hashedPassword = HashPassword(password);
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("ResetPassword", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", username);
					command.Parameters.AddWithValue("inputpass", hashedPassword);
					var result = command.ExecuteNonQuery();
				}
			}

		}
		public OTP GetOTP(string username)
		{
			OTP oTPDto = null; 

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("GetOTP", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("inputname", username);
					command.Parameters.AddWithValue("inputemail", username);

					using (var reader = command.ExecuteReader()) 
					{
						if (reader.Read()) 
						{
							oTPDto = new OTP
							{
								Value = reader.GetString(reader.GetOrdinal("OTP")), 
								Date = reader.GetDateTime(reader.GetOrdinal("Time")), 
							};
						}
					}
				}
			}

			return oTPDto; 
		}
		public void ResetPassRequest(string email, string otp)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("ResetPassRequest", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("inputemail", email); 
					command.Parameters.AddWithValue("OTPvalue", otp);    
					command.Parameters.AddWithValue("inputdate", DateTime.UtcNow); 

					command.ExecuteNonQuery();
				}
			}
		}


	}

}
