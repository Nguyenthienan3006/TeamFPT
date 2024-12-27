using LoginProject.Data;
using LoginProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace LoginProject.Repositories
{
    public class UsersService
    {
        private readonly DatabaseHelper _dbHelper;

        public UsersService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<User>? GetAllUsers()
        {

            var users = new List<User>();

            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "GetAllUsers";
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Email = reader["email"].ToString(),
                        Password = reader["password"].ToString(),
                        Role = reader["role"].ToString(),
                        IsVerified = Convert.ToBoolean(reader["is_verified"]),
                        VerificationToken = reader["verification_token"]?.ToString(),
                        VerificationTokenExpiration = reader["verification_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["verification_token_expiration"])
                    : (DateTime?)null,
                        ResetPasswordToken = reader["reset_password_token"]?.ToString(),
                        ResetPasswordTokenExpiration = reader["reset_password_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["reset_password_token_expiration"])
                    : (DateTime?)null
                    };
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return users.Count > 0 ? users:null;
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
                parameter.ParameterName = "p_username";
                parameter.Value = username;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Email = reader["email"].ToString(),
                        Password = reader["password"].ToString(),
                        Role = reader["role"].ToString(),
                        IsVerified = Convert.ToBoolean(reader["is_verified"]),
                        VerificationToken = reader["verification_token"]?.ToString(),
                        VerificationTokenExpiration = reader["verification_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["verification_token_expiration"])
                    : (DateTime?)null,
                        ResetPasswordToken = reader["reset_password_token"]?.ToString(),
                        ResetPasswordTokenExpiration = reader["reset_password_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["reset_password_token_expiration"])
                    : (DateTime?)null
                    };
                    return user;
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
                command.CommandText = "GetUserByEmail";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_email";
                parameter.Value = email;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Email = reader["email"].ToString(),
                        Password = reader["password"].ToString(),
                        Role = reader["role"].ToString(),
                        IsVerified = Convert.ToBoolean(reader["is_verified"]),
                        VerificationToken = reader["verification_token"]?.ToString(),
                        VerificationTokenExpiration = reader["verification_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["verification_token_expiration"])
                    : (DateTime?)null,
                        ResetPasswordToken = reader["reset_password_token"]?.ToString(),
                        ResetPasswordTokenExpiration = reader["reset_password_token_expiration"] != DBNull.Value
                    ? Convert.ToDateTime(reader["reset_password_token_expiration"])
                    : (DateTime?)null
                    };
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;

            }
        }

        public User GetUserByVerificationToken(string token)
        {

            try
            {
                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "GetUserByVerificationToken";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_token";
                parameter.Value = token;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VerificationTokenExpiration = Convert.ToDateTime(reader["verification_token_expiration"]),
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

        public User GetUserByResetPasswordToken(string token)
        {

            try
            {
                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "GetUserByResetPasswordToken";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_token";
                parameter.Value = token;
                command.Parameters.Add(parameter);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        ResetPasswordTokenExpiration = Convert.ToDateTime(reader["reset_password_token_expiration"]),
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
                command.CommandText = "Login";
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
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Role = reader["role"].ToString(),
                        IsVerified = Convert.ToBoolean(reader["is_verified"])
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
                command.CommandText = "InsertUser";
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

                var isVerified = command.CreateParameter();
                isVerified.ParameterName = "p_is_verified";
                isVerified.Value = false;
                command.Parameters.Add(isVerified);

                var verificationToken = command.CreateParameter();
                verificationToken.ParameterName = "p_verification_token";
                verificationToken.Value = user.VerificationToken;
                command.Parameters.Add(verificationToken);

                var verification_token_expiration = command.CreateParameter();
                verification_token_expiration.ParameterName = "p_verification_token_expiration";
                verification_token_expiration.Value = user.VerificationTokenExpiration;
                command.Parameters.Add(verification_token_expiration);

                var resetPasswordToken = command.CreateParameter();
                resetPasswordToken.ParameterName = "p_reset_password_token";
                resetPasswordToken.Value = user.ResetPasswordToken;
                command.Parameters.Add(resetPasswordToken);

                var reset_password_token_expiration = command.CreateParameter();
                reset_password_token_expiration.ParameterName = "p_reset_password_token_expiration";
                reset_password_token_expiration.Value = user.ResetPasswordTokenExpiration;
                command.Parameters.Add(reset_password_token_expiration);

                connection.Open();

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool VerifyEmail(int userId)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "VerifyEmail";
                command.CommandType = CommandType.StoredProcedure;

                var id = command.CreateParameter();
                id.ParameterName = "p_id";
                id.Value = userId;
                command.Parameters.Add(id);

                connection.Open();

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateVerificationToken(User user)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "UpdateVerificationToken";
                command.CommandType = CommandType.StoredProcedure;

                var email = command.CreateParameter();
                email.ParameterName = "p_email";
                email.Value = user.Email;
                command.Parameters.Add(email);

                var tokenExpiration = command.CreateParameter();
                tokenExpiration.ParameterName = "p_token_expiration";
                tokenExpiration.Value = user.VerificationTokenExpiration;
                command.Parameters.Add(tokenExpiration);

                var token = command.CreateParameter();
                token.ParameterName = "p_verification_token";
                token.Value = user.VerificationToken;
                command.Parameters.Add(token);

                connection.Open();

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateResetPasswordToken(User user)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "UpdateResetPasswordToken";
                command.CommandType = CommandType.StoredProcedure;

                var email = command.CreateParameter();
                email.ParameterName = "p_email";
                email.Value = user.Email;
                command.Parameters.Add(email);

                var tokenExpiration = command.CreateParameter();
                tokenExpiration.ParameterName = "p_reset_password_token_expiration";
                tokenExpiration.Value = user.ResetPasswordTokenExpiration;
                command.Parameters.Add(tokenExpiration);

                var token = command.CreateParameter();
                token.ParameterName = "p_reset_password_token";
                token.Value = user.ResetPasswordToken;
                command.Parameters.Add(token);

                connection.Open();

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool ResetPassword(string token, string newPassword)
        {
            try
            {

                using var connection = _dbHelper.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "ResetPassword";
                command.CommandType = CommandType.StoredProcedure;

                var resetPassToken = command.CreateParameter();
                resetPassToken.ParameterName = "p_reset_password_token";
                resetPassToken.Value = token;
                command.Parameters.Add(resetPassToken);

                var password = command.CreateParameter();
                password.ParameterName = "p_new_password";
                password.Value = newPassword;
                command.Parameters.Add(password);

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
