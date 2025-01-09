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
									Role = reader.GetString("role")
								};

							}
						}
					}
				}
			}
			return currentUser;
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

		public User GetUserById(int userId)
		{
			User user = null;

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("GetUser", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Add the parameter for the user ID
					command.Parameters.AddWithValue("Pid", userId);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							user = new User
							{
								Id = reader.GetInt32("id"),
								Username = reader.GetString("name"),
								Email = reader.GetString("email"),
								Address = reader.GetString("address"),
								Phone = reader.GetString("phone"),
								Role = reader.GetString("role")
							};
						}
					}
				}
			}
			return user;
		}

		public int CheckRegister(string email, string name)
		{
			int result = 0; 

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("checkRegister", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("Pemail", email);
					command.Parameters.AddWithValue("Pname", name);
					
					result = Convert.ToInt32(command.ExecuteScalar()); 
				}
			}
			return result;
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

					command.Parameters.AddWithValue("Pname", model.Username);
					command.Parameters.AddWithValue("Ppassword", hashedPassword); 
					command.Parameters.AddWithValue("Pemail", model.Email);
					command.Parameters.AddWithValue("Paddress", model.Address);
					command.Parameters.AddWithValue("Pphone", model.Phone);
					command.Parameters.AddWithValue("OTPvalue", otp);
					command.Parameters.AddWithValue("Pdate", DateTime.UtcNow);

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

		public void VerifyUser(string email)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("VerifyUser", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("Pemail", email);
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
					command.Parameters.AddWithValue("Pemail", username);
					command.Parameters.AddWithValue("Ppass", hashedPassword);
					var result = command.ExecuteNonQuery();
				}
			}
		}

		public OTP GetOTP(string email, string type)
		{
			OTP oTPDto = null; 

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("GetOTP", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("Pemail", email);
					command.Parameters.AddWithValue("Ptype", type);


					using (var reader = command.ExecuteReader()) 
					{
						if (reader.Read()) 
						{
							oTPDto = new OTP
							{
								Value = reader.GetString(reader.GetOrdinal("value")), 
								Date = reader.GetDateTime(reader.GetOrdinal("date")), 
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

					command.Parameters.AddWithValue("Pemail", email); 
					command.Parameters.AddWithValue("OTPvalue", otp);    
					command.Parameters.AddWithValue("Pdate", DateTime.UtcNow); 

					command.ExecuteNonQuery();
				}
			}
		}
	}
}
