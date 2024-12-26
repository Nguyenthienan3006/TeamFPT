using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using Project_Swagger.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.WebHost.UseUrls("http://+:5000");



// Add services to the container
//=========================================================================================
builder.Services.AddControllers();
// Enable Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "Jwt",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
});
//=========================================================================================

// Đăng ký MySQL connection như một dịch vụ Singleton
builder.Services.AddSingleton<MySqlConnectionService>();
builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(connectionString));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var emailSettings = configuration.GetSection("EmailSettings");
    return new EmailService(
        emailSettings["SmtpServer"],
        int.Parse(emailSettings["SmtpPort"]),
        emailSettings["SmtpUser"],
        emailSettings["SmtpPassword"]
    );
});



//=========================================================================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });
//=========================================================================================

builder.Services.AddAuthorization();

var app = builder.Build();
// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
});
//=========================================================================================

// Middleware for authentication and authorization
app.UseAuthentication(); // Uncomment if authentication is configured
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();