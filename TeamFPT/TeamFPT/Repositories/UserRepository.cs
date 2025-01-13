using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TeamFPT.DTO;
using TeamFPT.Model;
using TeamFPT.Services;

namespace TeamFPT.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseConnection _dbConnection;
        public UserRepository(IConfiguration configuration)
        {
            var _connectionString = configuration.GetConnectionString("MySqlConnection");
            _dbConnection = new DatabaseConnection(_connectionString);
        }

        public UserAuthentication Login(string username, string password)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("Login", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Username", username);
            command.Parameters.AddWithValue("@p_Password", password);

            using var reader =  command.ExecuteReader();
            if (reader.Read())
            {
                var user = new Users
                {
                    UserId = reader.GetInt32("UserId"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName")
                };

                return new UserAuthentication
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    IsVerified = reader.GetBoolean("IsVerified"),
                    UserRole = reader.GetString("UserRole"),
                    User = user
                };
            }
            return null;
        }
        public void Register(RegisterRequest request)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("Register", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_FirstName", request.FirstName);
            command.Parameters.AddWithValue("@p_LastName", request.LastName);
            command.Parameters.AddWithValue("@p_Address", request.Address);
            command.Parameters.AddWithValue("@p_ProvinceID", request.ProvinceID);
            command.Parameters.AddWithValue("@p_DistrictID", request.DistrictID);
            command.Parameters.AddWithValue("@p_WardID", request.WardID);
            command.Parameters.AddWithValue("@p_Email", request.Email);
            command.Parameters.AddWithValue("@p_Username", request.Username);
            command.Parameters.AddWithValue("@p_Password", request.Password);
            command.Parameters.AddWithValue("@p_UserRole", request.UserRole);

            command.ExecuteNonQuery();
        }

        public void SaveOtp(string email, string otp)
        {
            CleanupExpiredOtp();
            using var connection = _dbConnection.GetConnection();
            var command = new MySqlCommand("SaveOtp", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Otp", otp);
            command.Parameters.AddWithValue("@p_Email", email);
            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0) throw new Exception("Failed to save OTP. Email not found.");
        }

        public bool IsUniqueEmail(string email)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("IsUniqueEmail", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Email", email);
            return (long)command.ExecuteScalar() == 0;
        }
        public List<Provinces> GetAllProvinces()
        {
            using var connection = _dbConnection.GetConnection();
            var provinces = new List<Provinces>();

            using (var command = new MySqlCommand("getAllProvinces", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        provinces.Add(new Provinces
                        {
                            ProvinceID = reader.GetInt32("ProvinceID"),
                            ProvinceName = reader.GetString("ProvinceName")
                        });
                    }
                }
            }

            return provinces;
        }
        public List<Districts> GetDistrictsByProvinceId(int provinceId)
        {
            using var connection = _dbConnection.GetConnection();
            var districts = new List<Districts>();

            using (var command = new MySqlCommand("getDistrictByProvinceID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_ProvinceID", provinceId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        districts.Add(new Districts
                        {
                            DistrictID = reader.GetInt32("DistrictID"),
                            DistrictName = reader.GetString("DistrictName")
                        });
                    }
                }
            }

            return districts;
        }
        public List<Wards> GetWardsByDistrictId(int districtId)
        {
            using var connection = _dbConnection.GetConnection();
            var wards = new List<Wards>();

            using (var command = new MySqlCommand("GetWardsByDistrictId", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_DistrictID", districtId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        wards.Add(new Wards
                        {
                            WardID = reader.GetInt32("WardID"),
                            WardName = reader.GetString("WardName"),
                        });
                    }
                }
            }

            return wards;
        }

        public bool CheckEmailExists(string email)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("CheckEmailExists", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Email", email);
            return (long)command.ExecuteScalar() > 0;
        }

        public ValidationResults ValidateUser(RegisterRequest user)
        {
            var result = new ValidationResults();

            if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
                result.Errors.Add("Invalid email format.");

            if (user.Username.Length < 5 || user.Username.Length > 50)
                result.Errors.Add("Username must be between 5 and 50 characters.");

            if (!IsUniqueEmail(user.Email))
                result.Errors.Add("Email is already registered.");

            if (!IsUniqueUsername(user.Username))
                result.Errors.Add("Username is already taken.");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        private bool IsUniqueUsername(string username)
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("CheckUsernameExists", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Username", username);
            return (long)command.ExecuteScalar() == 0;
        }
        public bool VerifyOtp(string email, string otp)
        {
            using var connection = _dbConnection.GetConnection();
            int tokenId;
            using (var command = new MySqlCommand(
                "SELECT t.TokenId, t.Expiration " +
                "FROM Tokens t " +
                "JOIN UserAuthentication u ON t.UserId = u.UserId " +
                "WHERE u.Email = @Email AND t.TokenValue = @Otp AND t.Expiration > NOW()", connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Otp", otp);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return false; 
                }
                tokenId = reader.GetInt32("TokenId");
            }
            using (var updateCommand = new MySqlCommand(
                "UPDATE UserAuthentication SET IsVerified = 1 WHERE UserId = " +
                "(SELECT UserId FROM Tokens WHERE TokenId = @TokenId)", connection))
            {
                updateCommand.Parameters.AddWithValue("@TokenId", tokenId);
                updateCommand.ExecuteNonQuery();
            }
            using (var deleteCommand = new MySqlCommand("DELETE FROM Tokens WHERE TokenId = @TokenId", connection))
            {
                deleteCommand.Parameters.AddWithValue("@TokenId", tokenId);
                deleteCommand.ExecuteNonQuery();
            }

            return true;
        }

        public void CleanupExpiredOtp()
        {
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("CleanupExpiredOtp", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
        }
        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using var connection = _dbConnection.GetConnection();
            var command = new MySqlCommand("ChangePassword", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_UserId", userId);
            command.Parameters.AddWithValue("@p_OldPassword", oldPassword);
            command.Parameters.AddWithValue("@p_NewPassword", newPassword);

            command.ExecuteNonQuery();
        }
        public void SaveResetPasswordOtp(string email, string otp)
        {
            CleanupExpiredOtp();
            using var connection = _dbConnection.GetConnection();
            using var command = new MySqlCommand("SaveResetPasswordOtp", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("p_Email", email);
            command.Parameters.AddWithValue("p_Otp", otp);
            command.ExecuteNonQuery();
        }

        public bool VerifyResetPasswordOtp(string email, string otp)
        {
            using var connection = _dbConnection.GetConnection();
            var command = new MySqlCommand(
                "SELECT u.UserId FROM UserAuthentication u " +
                "JOIN Tokens t ON u.UserId = t.UserId " +
                "WHERE u.Email = @Email AND t.TokenValue = @Otp AND t.TokenType = 'ResetPassword' AND t.Expiration > NOW()", connection);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Otp", otp);

            var userId = command.ExecuteScalar();
            if (userId != null)
            {
                var deleteCommand = new MySqlCommand(
                    "DELETE FROM Tokens WHERE TokenValue = @Otp", connection);
                deleteCommand.Parameters.AddWithValue("@Otp", otp);
                deleteCommand.ExecuteNonQuery();
                return true;
            }

            return false;
        } 

        public void UpdatePassword(string email, string newPassword)
        {
            using var connection = _dbConnection.GetConnection();
            var command = new MySqlCommand("UpdatePassword", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Email", email);
            command.Parameters.AddWithValue("@p_NewPassword", newPassword);

            command.ExecuteNonQuery();
        }
        public Provinces GetProvinceById(int provinceId)
        {
            using var connection = _dbConnection.GetConnection();

            using (var command = new MySqlCommand("GetProvinceById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_ProvinceID", provinceId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Provinces
                        {
                            ProvinceID = reader.GetInt32("ProvinceID"),
                            ProvinceName = reader.GetString("ProvinceName")
                        };
                    }
                }
            }

            return null; // Nếu không tìm thấy
        }
        public Districts GetDistrictById(int districtId)
        {
            using var connection = _dbConnection.GetConnection();

            using (var command = new MySqlCommand("GetDistrictById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_DistrictID", districtId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Districts
                        {
                            DistrictID = reader.GetInt32("DistrictID"),
                            DistrictName = reader.GetString("DistrictName"),
                            ProvinceID = reader.GetInt32("ProvinceID")
                        };
                    }
                }
            }

            return null;
        }
        public Wards GetWardById(int wardId)
        {
            using var connection = _dbConnection.GetConnection();

            using (var command = new MySqlCommand("GetWardById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_WardID", wardId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Wards
                        {
                            WardID = reader.GetInt32("WardID"),
                            WardName = reader.GetString("WardName"),
                            DistrictID = reader.GetInt32("DistrictID")
                        };
                    }
                }
            }

            return null;
        }

    }
}
