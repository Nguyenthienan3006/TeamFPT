using LoginProject.Data;
using LoginProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace LoginProject.Repositories
{
    public class UsersRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public UsersRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<User> GetAllUsers()
        {

            var users = new List<User>();

            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "sp_GetAllUsers";
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new Models.User
                    {
                        Id = Convert.ToInt32(reader["user_id"]),
                        Username = reader["username"].ToString(),
                        Email = reader["email"].ToString(),
                        Role = reader["role"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return users;
        }

        public User GetUserByUsername(string username)
        {

            try
            {
                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "GetUserByUsername";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "inputUsername";
                parameter.Value = username;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = Convert.ToInt32(reader["user_id"]),
                        Username = reader["username"].ToString(),
                        Password = reader["password"].ToString(),
                        Email = reader["email"].ToString(),
                        Role = reader["role"].ToString()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;

            }
        }

        public User GetUserByEmail(string email)
        {

            try
            {
                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "sp_GetUserByEmail";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_email";
                parameter.Value = email;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = Convert.ToInt32(reader["user_id"]),
                        Username = reader["username"].ToString(),
                        Password = reader["password"].ToString(),
                        Email = reader["email"].ToString(),
                        Role = reader["role"].ToString()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;

            }
        }


        public User AuthenticateUser(string username, string password)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "sp_Login";
                command.CommandType = CommandType.StoredProcedure;



                var usernameParameter = command.CreateParameter();
                usernameParameter.ParameterName = "p_username";
                usernameParameter.Value = username;
                command.Parameters.Add(usernameParameter);

                var passwordParameter = command.CreateParameter();
                passwordParameter.ParameterName = "p_password";
                passwordParameter.Value = password;
                command.Parameters.Add(passwordParameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {

                    return new User
                    {
                        Id = Convert.ToInt32(reader["user_id"]),
                        Username = reader["username"].ToString(),
                        Role = reader["role"].ToString()
                    };

                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        public bool Register(User user)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "sp_InsertUser";
                command.CommandType = CommandType.StoredProcedure;

                var username = command.CreateParameter();
                username.ParameterName = "p_username";
                username.Value = user.Username;
                command.Parameters.Add(username);

                var password = command.CreateParameter();
                password.ParameterName = "p_password";
                password.Value = user.Password;
                command.Parameters.Add(password);

                var email = command.CreateParameter();
                email.ParameterName = "p_email";
                email.Value = user.Email;
                command.Parameters.Add(email);

                var role = command.CreateParameter();
                role.ParameterName = "p_role";
                role.Value = "user";
                command.Parameters.Add(role);

                connection.Open();

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
