using DailyCheckBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyCheckBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DailyCheckDbContext _dailyCheckDbContext;

        public LoginController(DailyCheckDbContext dailyCheckDbContext)
        {
            _dailyCheckDbContext = dailyCheckDbContext;
        }

        // POST: api/login/register
        [HttpPost("register")]
        public IActionResult Registration([FromBody] RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Checking if Email already exists in the database
                if (_dailyCheckDbContext.Users.Any(u => u.Email == model.Email))
                {
                    return BadRequest(new { message = "Login! u already have an account" });
                }

                // Create a new user
                var passwordHasher = new PasswordHasher<User>();
                var user = new User
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    Password = passwordHasher.HashPassword(null, model.Password), // Hashing the password
                    Role = Role.User
                };

                try
                {
                    // Add the new user to the database
                    _dailyCheckDbContext.Users.Add(user);
                    _dailyCheckDbContext.SaveChanges();
                    return Ok(new { message = "User registered successfully." });
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);

                    if (ex.InnerException != null)
                    {
                        var sqlException = ex.InnerException as Npgsql.PostgresException;
                        if (sqlException != null && sqlException.SqlState == "23505")
                        {
                            // Handle unique constraint violation (e.g., email already exists)
                            Console.WriteLine("Error: " + ex.Message);
                            return BadRequest(new { message = "This email is already in use." });
                        }
                        else
                        {
                            return StatusCode(500, "An unexpected error occurred while saving your information.");
                        }
                    }
                    else
                    {
                        return StatusCode(500, "An unexpected error occurred while saving your information.");
                    }
                }
            }

            return BadRequest("Invalid registration data.");
        }

        // POST: api/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Authentication by username or email
                var user = _dailyCheckDbContext.Users.FirstOrDefault(u =>
                    u.UserName.ToLower().Trim() == model.UserNameOrEmail.ToLower().Trim() ||
                    u.Email.ToLower().Trim() == model.UserNameOrEmail.ToLower().Trim());

                if (user != null)
                {
                    var passwordHasher = new PasswordHasher<User>();


                    var result = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        // Authentication successful
                        return Ok(new { message = "Login successful" });
                    }
                    else
                    {
                        // Password is incorrect
                        return BadRequest(new { message = "Password is incorrect" });
                    }
                }
                else
                {
                    // User not found
                    return BadRequest(new { message = "User was not found" });
                }
            }

            return BadRequest();
        }

        // Google login - Check if the email exists in the database
        [HttpPost("loginwithgoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] loginWithGoogleViewModel googleOauthData)
        {
            if (googleOauthData == null || string.IsNullOrEmpty(googleOauthData.Email))
            {
                // No email provided in the request
                return BadRequest("Email not provided.");
            }

            var user = await _dailyCheckDbContext.Users
                        .Where(u => u.Email == googleOauthData.Email)
                        .FirstOrDefaultAsync();

            var googleUser = await _dailyCheckDbContext.GoogleUsers
                        .Where(u => u.Email == googleOauthData.Email)
                        .FirstOrDefaultAsync();

            // If no Google user exists and the email is not in the Users table
            if (googleUser == null && user?.Email != googleOauthData.Email)
            {
                // Create a new Google user
                var newGoogleUser = new GoogleUser
                {
                    Id = googleOauthData.Id,
                    Username = googleOauthData.Username,
                    Email = googleOauthData.Email,
                    Role = Role.User // Assign the 'User' role
                };
                _dailyCheckDbContext.GoogleUsers.Add(newGoogleUser);
                _dailyCheckDbContext.SaveChanges();
                return Ok(new { exists = false });
            }
            else if (googleOauthData.Email == user?.Email)
            {
                // If it's a regular user, return that the email is already in use

                return Ok(new { exists = false });
            }
            else
            {
                // If a Google user already exists
                return Ok(new { exists = true });
            }
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "User logged out successfully." });
        }
    }
}
