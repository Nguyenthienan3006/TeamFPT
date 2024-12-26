using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TeamFPT.Repositories;
using TeamFPT.Services;

namespace TeamFPT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<JwtTokenGenerator>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddControllers();

            
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

            builder.Services.AddAuthentication(
                 options =>
                 {
                     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                 }
                 ).AddJwtBearer(options =>
                 {
                     options.RequireHttpsMetadata = false;
                     options.SaveToken = true;
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
                 }
                 );
            builder.Services.AddAuthorization();
            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

  
            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
