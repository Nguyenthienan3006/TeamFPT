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


		public void RegisterUser(RegisterRequestModel requestModel)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("Register", connection))  
				{
					command.CommandType = CommandType.StoredProcedure;

					// Add the parameters: name, password, email
					command.Parameters.AddWithValue("registername", requestModel.Username);
					command.Parameters.AddWithValue("registerpassword", requestModel.Password);
					command.Parameters.AddWithValue("registeremail", requestModel.Email);

					var result = command.ExecuteNonQuery();  
				}
			}

		}

		public void VerifyUser(string username, string pass)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new MySqlCommand("VerifySuccess", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("inputname", username);
					command.Parameters.AddWithValue("inputpass", pass);

					var result = command.ExecuteNonQuery();
				}
			}

		}
	}

}
