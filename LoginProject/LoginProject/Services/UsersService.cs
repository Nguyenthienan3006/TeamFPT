using Dapper;
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

        //public List<User>? GetAllUsers()
        //{

        //    var users = new List<User>();

        //    try
        //    {

        //        using var connection = _dbHelper.CreateConnection();
        //        using var command = connection.CreateCommand();
        //        command.CommandText = "GetAllUsers";
        //        command.CommandType = CommandType.StoredProcedure;

        //        connection.Open();
        //        using var reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            var user = new User
        //            {
        //                Id = Convert.ToInt32(reader["id"]),
        //                Username = reader["username"].ToString(),
        //                Email = reader["email"].ToString(),
        //                Password = reader["password"].ToString(),
        //                Role = reader["role"].ToString(),
        //                IsVerified = Convert.ToBoolean(reader["is_verified"]),
        //                VerificationToken = reader["verification_token"]?.ToString(),
        //                VerificationTokenExpiration = reader["verification_token_expiration"] != DBNull.Value
        //            ? Convert.ToDateTime(reader["verification_token_expiration"])
        //            : (DateTime?)null,
        //                ResetPasswordToken = reader["reset_password_token"]?.ToString(),
        //                ResetPasswordTokenExpiration = reader["reset_password_token_expiration"] != DBNull.Value
        //            ? Convert.ToDateTime(reader["reset_password_token_expiration"])
        //            : (DateTime?)null
        //            };
        //            users.Add(user);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return users.Count > 0 ? users : null;
        //}

        public User? GetUserByUsername(string username)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_username", username);

            return _dbHelper.ExecuteStoredProcedureSingle<User>("GetUserByUsername", parameters);
        }

        public User? GetUserByEmail(string email)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_email", email);

            return _dbHelper.ExecuteStoredProcedureSingle<User>("GetUserByEmail", parameters);
        }

        public bool Register(User user,string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_username", user.Username);
            parameters.Add("p_email", user.Email);
            parameters.Add("p_password_hash", user.PasswordHash);
            parameters.Add("p_verification_token", token);

            return _dbHelper.ExecuteStoredProcedure("Register", parameters);
        }

        public bool VerifyEmail(string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_verification_token", token);

            return _dbHelper.ExecuteStoredProcedure("VerifyEmail",parameters);
        }

        public bool InsertVerificationToken(string email, string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_email", email);
            parameters.Add("p_new_token",token);

            return _dbHelper.ExecuteStoredProcedure("InsertVerificationToken", parameters);
        }

        public bool InsertResetPasswordToken(string email, string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_email", email);
            parameters.Add("p_token", token);

            return _dbHelper.ExecuteStoredProcedure("InsertResetPasswordToken", parameters);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_reset_password_token", token);
            parameters.Add("p_new_password", newPassword);

            return _dbHelper.ExecuteStoredProcedure("ResetPassword", parameters);
        }

        public User? ValidateUser(string username, string password)
        {
            var user = GetUserByUsername(username);

            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            return user;
        }
    }
}
