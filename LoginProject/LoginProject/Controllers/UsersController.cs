﻿using LoginProject.Data;
using LoginProject.Models;
using LoginProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Linq.Expressions;

namespace LoginProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        //[HttpGet("get-all")]
        //public IActionResult GetAllUsers()
        //{
        //    try
        //    {
        //        var users = _usersService.GetAllUsers();

        //        if (users == null)
        //        {
        //            return NotFound("No users found");
        //        }
        //        return Ok(users);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //[HttpGet("{username}")]
        //[Authorize]
        //public IActionResult GetUserByUsername(string username)
        //{
        //    try
        //    {
        //        var user = _usersService.GetUserByUsername(username);

        //        if (user == null)
        //        {
        //            return NotFound();
        //        }
        //        return Ok(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }

        //}
    }
}