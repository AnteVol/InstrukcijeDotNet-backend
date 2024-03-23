using InstrukcijeDotNet.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InstrukcijeDotNet
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            SetUpDB(builder);
            SetUpJWT(builder);
            // var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";



            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            TestDatabaseConnection(app.Services);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication(); //has to go before UseAuthorization
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void SetUpDB(WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AppContextHandler>(options =>
                            options.UseSqlite(builder.Configuration.GetConnectionString("AppContextExampleConnection")));
        }

        private static void SetUpJWT(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization();
        }
        private async static void TestDatabaseConnection(IServiceProvider services)
            {
                await Task.Delay(3000);

                using (var scope = services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppContextHandler>();
                    try
                    {
                        dbContext.Database.OpenConnection();
                        dbContext.Database.CloseConnection();
                        Console.WriteLine("Database connection successful.");
                
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database connection failed: {ex.Message}");
                    }
                }
            }
    }
    
}