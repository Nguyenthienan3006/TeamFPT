using UserManagementAPI.Models;
using UserManagementAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


// Đăng ký UserStore
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton(new EmailService(
    builder.Configuration["EmailSettings:SmtpServer"],
    int.Parse(builder.Configuration["EmailSettings:SmtpPort"]),
    builder.Configuration["EmailSettings:SmtpUser"],
    builder.Configuration["EmailSettings:SmtpPassword"]
));
// Cấu hình JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Services.AddDistributedRedisCache(
    options =>
    {
        options.Configuration = "localhost:7127";

    }
    );
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
