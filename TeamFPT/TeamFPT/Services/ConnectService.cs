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
				using (var command = new MySqlCommand("sp_Login", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("p_username", userLogin.UserName);
					command.Parameters.AddWithValue("p_password", userLogin.PassWord);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							currentUser = new User
							{
								Id = reader.GetInt32("user_id"),
								Username = reader.GetString("username"),

								Email = reader.GetString("email"),
								Role = reader.GetString("role")
							};
						}
					}
				}
			}

			return currentUser;
		}
	}
}
