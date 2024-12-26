using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TeamFPT.Services;

namespace TeamFPT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddScoped<ConnectService>();
			builder.Services.AddScoped<EmailService>();
			builder.Services.AddScoped<JwtService>();
			
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

			
			builder.Services.AddAuthentication (
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }
                ).AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata= false;
                    options.SaveToken   =  true;
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
				});
			}

            app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
