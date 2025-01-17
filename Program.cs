using DailyCheckBackend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
            })
            .AddGoogle(
                GoogleDefaults.AuthenticationScheme,
                options =>
                {
                    {
                        var clientId = builder
                            .Configuration.GetSection("GoogleKeys:ClientId")
                            .Value;
                        var clientSecret = builder
                            .Configuration.GetSection("GoogleKeys:ClientSecret")
                            .Value;

                        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                        {
                            throw new InvalidOperationException(
                                "Les clés ClientId et ClientSecret sont manquantes dans la configuration."
                            );
                        }

                        options.ClientId = clientId;
                        options.ClientSecret = clientSecret;
                    }
                }
            );

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DailyCheckDbContext>(o =>
            o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        var app = builder.Build();

        // Configurez le pipeline de requêtes HTTP.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowAll");

        // app.UseSession(); // Assurez-vous que UseSession est avant UseAuthorization

        app.UseAuthorization();

        // Mappez les contrôleurs
        app.MapControllers();

        // Exemple de point de terminaison
        // var summaries = new[]
        // {
        //     "Freezing",
        //     "Bracing",
        //     "Chilly",
        //     "Cool",
        //     "Mild",
        //     "Warm",
        //     "Balmy",
        //     "Hot",
        //     "Sweltering",
        //     "Scorching",
        // };

        // app.MapGet(
        //         "/weatherforecast",
        //         () =>
        //         {
        //             var forecast = Enumerable
        //                 .Range(1, 5)
        //                 .Select(index => new WeatherForecast(
        //                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //                     Random.Shared.Next(-20, 55),
        //                     summaries[Random.Shared.Next(summaries.Length)]
        //                 ))
        //                 .ToArray();
        //             return forecast;
        //         }
        //     )
        //     .WithName("GetWeatherForecast")
        //     .WithOpenApi();

        app.Run();
    }
}

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
