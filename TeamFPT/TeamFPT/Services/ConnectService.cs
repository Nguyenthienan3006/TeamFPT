using Microsoft.AspNetCore.Mvc;
using System.Data;
using TeamFPT.Models;
using TeamFPT.Models.API;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

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

			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand("GetUser", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", userLogin.UserName);
					command.Parameters.AddWithValue("inputpass", userLogin.PassWord);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							currentUser = new User
							{
								Id = reader.GetInt32("id"),
								Username = reader.GetString("name"),
								Email = reader.GetString("email"),
								Role = reader.GetString("role"),
								IsValid = reader.GetBoolean("isvalid")
							};
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
				using (var command = new MySqlCommand("GetEmail", connection))
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


		public void RegisterUser(string name, string pass, string email, string otp)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("Register", connection))  
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("registername", name);
					command.Parameters.AddWithValue("registerpassword", pass);
					command.Parameters.AddWithValue("registeremail", email);
					command.Parameters.AddWithValue("OTPvalue", otp);
					command.Parameters.AddWithValue("inputdate", DateTime.UtcNow);

					var result = command.ExecuteNonQuery();  
				}
			}

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
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("ResetPassword", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("inputname", username);
					command.Parameters.AddWithValue("inputpass", password);
					var result = command.ExecuteNonQuery();
				}
			}

		}
		public OTPDto GetOTP(string username)
		{
			OTPDto oTPDto = null; 

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
							oTPDto = new OTPDto
							{
								OTP = reader.GetString(reader.GetOrdinal("OTP")), 
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
