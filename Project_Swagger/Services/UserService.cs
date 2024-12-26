using System.Data;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Project_Swagger.DTO;
using Project_Swagger.Models;

namespace Project_Swagger.Services
{
    public class UserService
    {
        private readonly MySqlConnectionService _connectionService;

        public UserService(MySqlConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public User GetAnUser(string username, string password)
        {
            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            if (!username.IsNullOrEmpty())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "FindUser";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new MySqlParameter("p_field", MySqlDbType.VarChar) { Value = "username" });
                command.Parameters.Add(new MySqlParameter("p_value", MySqlDbType.VarChar) { Value = username });

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {

                    if (password.Equals(reader["Password"].ToString()))
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Username = reader["Username"].ToString(),
                            Fullname = reader["Fullname"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString(),
                            IsEmailVerified = Convert.ToBoolean(reader["isEmailVerified"])
                        };
                    }
                }
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            return null;
        }

        public User GetUserByEmail(string email)
        {
            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            if (!email.IsNullOrEmpty())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "FindUser";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new MySqlParameter("p_field", MySqlDbType.VarChar) { Value = "email" });
                command.Parameters.Add(new MySqlParameter("p_value", MySqlDbType.VarChar) { Value = email });

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        Username = reader["Username"].ToString(),
                        Fullname = reader["Fullname"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString()
                    };
                }
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            return null;
        }

        public bool AddAnUser(UserRegisterDTO userRegisterDTO)
        {
            bool result = false;
            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "UserRegister";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("p_username", MySqlDbType.VarChar) { Value = userRegisterDTO.UserName });
            command.Parameters.Add(new MySqlParameter("p_password", MySqlDbType.VarChar) { Value = userRegisterDTO.Password });
            command.Parameters.Add(new MySqlParameter("p_fullname", MySqlDbType.VarChar) { Value = userRegisterDTO.Fullname });
            command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = userRegisterDTO.Email });
            command.Parameters.Add(new MySqlParameter("p_role", MySqlDbType.VarChar) { Value = userRegisterDTO.Role });
            command.Parameters.Add(new MySqlParameter("p_mailStatus", MySqlDbType.Bit) { Value = userRegisterDTO.IsEmailVerified });
            

            // Thêm tham số OUT để lấy kết quả
            var statusParam = new MySqlParameter("p_status", MySqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusParam);

            command.ExecuteNonQuery();

            result = Convert.ToBoolean(statusParam.Value);

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            return result;
        }

        public User GetUserByOTP(string otp)
        {
            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            if (!otp.IsNullOrEmpty())
            {
                using var command = connection.CreateCommand();
                command.CommandText = "FindUserByOTP";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new MySqlParameter("p_value", MySqlDbType.VarChar) { Value = otp });

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        IsEmailVerified = Convert.ToBoolean(reader["isEmailVerified"])
                    };
                }
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            return null;
        }

        public bool UpdateEmailVerified(int userId)
        {
            bool result = false;

            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "UpdateEmailVerified";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("userId", MySqlDbType.Int32) { Value = userId });

            var statusParam = new MySqlParameter("status", MySqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusParam);

            try
            {
                command.ExecuteNonQuery();
                result = Convert.ToBoolean(statusParam.Value); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return result;
        }


    }
}
