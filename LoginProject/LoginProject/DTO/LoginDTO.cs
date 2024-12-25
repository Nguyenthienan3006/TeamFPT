﻿using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 50 characters.")]
        public string Password { get; set; }
    }
}
