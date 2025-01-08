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

        public Account GetAnUserAccount(string username, string password)
        {
            using var connection = _connectionService.GetConnection();
            if (username.IsNullOrEmpty()) return null;
            if (connection.State != ConnectionState.Open) connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "FindUserAccount";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("p_username", MySqlDbType.VarChar) { Value = username });
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                if (password.Equals(reader["password_hash"].ToString()))
                {
                    return new Account
                    {
                        Username = reader["Username"].ToString(),
                        IsVerify = Convert.ToBoolean(reader["IsVerify"]),
                        User = new User
                        {
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString()
                        }
                    };
                }
            }
            return null;
        }

        public bool RegisterUser(UserRegisterDTO userRegisterDTO)
        {
            using (var connection = _connectionService.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "RegisterUser";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_username", MySqlDbType.VarChar) { Value = userRegisterDTO.UserName });
                        command.Parameters.Add(new MySqlParameter("p_fullname", MySqlDbType.VarChar) { Value = userRegisterDTO.Fullname });
                        command.Parameters.Add(new MySqlParameter("p_password", MySqlDbType.VarChar) { Value = userRegisterDTO.Password });
                        command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = userRegisterDTO.Email });
                        command.Parameters.Add(new MySqlParameter("p_role", MySqlDbType.VarChar) { Value = userRegisterDTO.Role });
                        command.Parameters.Add(new MySqlParameter("p_type_code", MySqlDbType.VarChar) { Value = userRegisterDTO.TypeCode });
                        command.Parameters.Add(new MySqlParameter("p_otp_code", MySqlDbType.VarChar) { Value = userRegisterDTO.OTP });
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool VerifyAccount(string otp, string email)
        {
            using (var connection = _connectionService.GetConnection())
            {
                
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "VerifyEmail";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = email });
                        command.Parameters.Add(new MySqlParameter("p_otp", MySqlDbType.VarChar) { Value = otp });
                        var outParam = new MySqlParameter("o_status", MySqlDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outParam);
                        command.ExecuteNonQuery();
                        return Convert.ToBoolean(outParam.Value);
                    }
                

            }
        }

        public bool ResendOTP(ResendOTPDTO resendOTPDTO)
        {
            using (var connection = _connectionService.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "ResendOTP";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = resendOTPDTO.email });
                        command.Parameters.Add(new MySqlParameter("p_otp", MySqlDbType.VarChar) { Value = resendOTPDTO.OTP });
                        command.Parameters.Add(new MySqlParameter("p_type_code", MySqlDbType.VarChar) { Value = resendOTPDTO.TypeCode });
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        
        public bool PasswordOTP(ChangePasswordDTO changePasswordDTO)
        {
            using (var connection = _connectionService.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "ResendOTP";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = changePasswordDTO.email });
                        command.Parameters.Add(new MySqlParameter("p_otp", MySqlDbType.VarChar) { Value = changePasswordDTO.OTP });
                        command.Parameters.Add(new MySqlParameter("p_type_code", MySqlDbType.VarChar) { Value = changePasswordDTO.TypeCode });
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool ChangePassword(UserChangerPasswordDTO userChangerPasswordDTO, string email)
        {
            using (var connection = _connectionService.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "ChangePassword";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar) { Value = email });
                        command.Parameters.Add(new MySqlParameter("p_otp", MySqlDbType.VarChar) { Value = userChangerPasswordDTO.OTP });
                        command.Parameters.Add(new MySqlParameter("p_password", MySqlDbType.VarChar) { Value = userChangerPasswordDTO.Password });
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
