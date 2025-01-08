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

        public List<User>? GetPagedUsers(int pageNumber, int pageSize)
        {
            var parameters = new DynamicParameters();
            parameters.Add("PageNumber", pageNumber);
            parameters.Add("PageSize", pageSize);

            return _dbHelper.ExecuteStoredProcedure<User>("GetPagedUsers", parameters).ToList();
        }

        public int GetTotalUserCount()
        {
            return _dbHelper.ExecuteStoredProcedureSingle<int>("GetTotalUserCount");
        }
        public List<User>? GetAllUsers()
        {
            return _dbHelper.ExecuteStoredProcedure<User>("GetAllUsers").ToList();
        }

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

        public bool Register(User user, string token)
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

            return _dbHelper.ExecuteStoredProcedure("VerifyEmail", parameters);
        }

        public bool InsertVerificationToken(string email, string token)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_email", email);
            parameters.Add("p_new_token", token);

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
            if (!VerifyPassword(password, user.PasswordHash)) return null;

            return user;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password,string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
