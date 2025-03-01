using DailyCheckBackend.Models;
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
        public async Task<IActionResult> Registration([FromBody] RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _dailyCheckDbContext.Users.FirstOrDefault(u =>
                    u.Email.ToLower().Trim() == model.Email.ToLower().Trim()
                );
                var existingGoogleUser = _dailyCheckDbContext.GoogleUsers.FirstOrDefault(u =>
                    u.Email.ToLower().Trim() == model.Email.ToLower().Trim()
                );
                // Checking if Email already exists in the database
                if (existingUser != null || existingGoogleUser != null)
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
                    Role = model.Role ?? Role.User,
                };

                try
                {
                    // Add the new user to the database
                    _dailyCheckDbContext.Users.Add(user);
                    await _dailyCheckDbContext.SaveChangesAsync();
                    return Ok(new { message = "User registered successfully.", user });
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
                            return StatusCode(
                                500,
                                "An unexpected error occurred while saving your information."
                            );
                        }
                    }
                    else
                    {
                        return StatusCode(
                            500,
                            "An unexpected error occurred while saving your information."
                        );
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
                    u.UserName.ToLower().Trim() == model.UserNameOrEmail.ToLower().Trim()
                    || u.Email.ToLower().Trim() == model.UserNameOrEmail.ToLower().Trim()
                );

                if (user != null)
                {
                    var passwordHasher = new PasswordHasher<User>();

                    var result = passwordHasher.VerifyHashedPassword(
                        user,
                        user.Password,
                        model.Password
                    );

                    if (result == PasswordVerificationResult.Success)
                    {
                        // Authentication successful

                        return Ok(new { message = "Login successful", user });
                    }
                    else
                    {
                        if (ModelState.IsValid)
                            // Password is incorrect
                            return BadRequest(new { message = "Password or Email is incorrect!" });
                    }
                }
                else
                {
                    // User not found
                    return BadRequest(new { message = "U don't have an account make one!" });
                }
            }

            return BadRequest();
        }

        // Google login - Check if the email exists in the database
        [HttpPost("loginwithgoogle")]
        public async Task<IActionResult> LoginWithGoogle(
            [FromBody] loginWithGoogleViewModel googleOauthData
        )
        {
            if (googleOauthData == null || string.IsNullOrEmpty(googleOauthData.Email))
            {
                // No email provided in the request
                return BadRequest("Email not provided.");
            }

            var user = await _dailyCheckDbContext
                .Users.Where(u => u.Email == googleOauthData.Email)
                .FirstOrDefaultAsync();

            var googleUser = await _dailyCheckDbContext
                .GoogleUsers.Where(u => u.Email == googleOauthData.Email)
                .FirstOrDefaultAsync();

            // If user  does not exists in db
            if (googleUser == null && user == null)
            {
                return NotFound(new { message = "You do not have an account yet!" });
            }
            if (user == null)
            {
                return Ok(new { exists = true, googleUser });
            }
            return Ok(new { exists = true, user });
        }

        [HttpPost("signupwithgoogle")]
        public async Task<IActionResult> SignUpGoogle(
            [FromBody] loginWithGoogleViewModel googleOauthData
        )
        {
            if (googleOauthData == null || string.IsNullOrEmpty(googleOauthData.Email))
            {
                // No email provided in the request
                return BadRequest("Email not provided.");
            }

            var user = await _dailyCheckDbContext
                .Users.Where(u => u.Email == googleOauthData.Email)
                .FirstOrDefaultAsync();

            var googleUser = await _dailyCheckDbContext
                .GoogleUsers.Where(u => u.Email == googleOauthData.Email)
                .FirstOrDefaultAsync();

            if (googleUser == null && user == null)
            {
                // Create a new Google user
                var newGoogleUser = new GoogleUser
                {
                    Id = googleOauthData.Id,
                    UserName = googleOauthData.Username,
                    Email = googleOauthData.Email,
                    Role =
                        Role.User // Assign the 'User' role
                    ,
                };
                _dailyCheckDbContext.GoogleUsers.Add(newGoogleUser);
                await _dailyCheckDbContext.SaveChangesAsync();
                return Ok(new { newGoogleUser });
            }

            return BadRequest(new { message = "Your already have an account! " });
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _dailyCheckDbContext.Users.ToList();
            var googleUsers = _dailyCheckDbContext.GoogleUsers.ToList();
            return Ok(new { users, googleUsers });
        }

        // GET: api/login/user/{id}
        [HttpGet("users/user")]
        public IActionResult GetUserById(string id)
        {
            var user = _dailyCheckDbContext.Users.Find(Convert.ToInt32(id));

            var googleUserId = Convert.ToString(id);
            var googleUser = _dailyCheckDbContext.GoogleUsers.Find(googleUserId);
            if (user == null && googleUser == null)
            {
                return NotFound(new { message = "User not found." });
            }
            else if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return Ok(googleUser);
            }
        }

        // PUT: api/login/user/{id}
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] User model)
        {
            // Console.WriteLine("this is the model" + model.Email);
            var user = _dailyCheckDbContext.Users.Find(model.Id);
            var googleUserId = Convert.ToString(model.Id);
            var googleUser = _dailyCheckDbContext.GoogleUsers.Find(googleUserId);
            if (user == null && googleUser == null)
            {
                return NotFound(new { message = "User not found." });
            }
            else if (user != null)
            {
                if (user.Email != model.Email)
                {
                    var newUserByEmail = _dailyCheckDbContext.Users.FirstOrDefault(u =>
                        u.Email == model.Email
                    );
                    if (newUserByEmail != null)
                    {
                        return BadRequest(new { message = "Email already in use!" });
                    }
                }
                user.UserName = model.UserName ?? user.UserName;
                user.Email = model.Email ?? user.Email;
                user.Role = model.Role;
                await _dailyCheckDbContext.SaveChangesAsync();
                return Ok(new { message = "User updated successfully.", user });
            }
            else
            {
                if (googleUser?.Email != model.Email)
                {
                    var newGoogleUserByEmail = _dailyCheckDbContext.GoogleUsers.FirstOrDefault(u =>
                        u.Email == model.Email
                    );
                    if (newGoogleUserByEmail != null)
                    {
                        return BadRequest(new { message = "Email already in use!" });
                    }
                }
                if (googleUser != null)
                {
                    googleUser.UserName = model.UserName ?? googleUser.UserName;
                    googleUser.Email = model.Email ?? googleUser.Email;
                    googleUser.Role = model.Role;
                }

                await _dailyCheckDbContext.SaveChangesAsync();
                return Ok(new { message = "User updated succesfully.", googleUser });
            }
        }

        // PUT: api/login/user/{id}
        [HttpPut("mainuser/update")]
        public async Task<IActionResult> UpdateMainUser([FromBody] UserUpdateViewModel model)
        {
            var user = _dailyCheckDbContext.Users.Find(model.Id);
            if (user != null && model.Password.Length <= 0)
            {
                if (user.Email != model.Email)
                {
                    var newUserByEmail = _dailyCheckDbContext.Users.FirstOrDefault(u =>
                        u.Email == model.Email
                    );
                    if (newUserByEmail != null)
                    {
                        return BadRequest(new { message = "Email already in use!" });
                    }
                }
                user.UserName = model.UserName ?? user.UserName;
                user.Email = model.Email ?? user.Email;
                await _dailyCheckDbContext.SaveChangesAsync();
                return Ok(new { message = "User updated successfully.", user });
            }
            var passwordHasher = new PasswordHasher<User>();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User can't be null");
            }
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                user.Password = model.NewPassword;
                return Ok(new { message = "The password is correct", user });
            }
            else if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("the password is incorrect");
                return BadRequest(new { message = "The password is incorrect!" });
            }
            return Ok();
        }

        public class DeleteUserRequest
        {
            public string? userId { get; set; }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            var googleUser = _dailyCheckDbContext.GoogleUsers.FirstOrDefault(gu =>
                gu.Id == request.userId
            );
            if (googleUser == null)
            {
                var usersId = Convert.ToInt32(request.userId);
                var user = _dailyCheckDbContext.Users.FirstOrDefault(u => u.Id == usersId);
                if (user != null)
                    _dailyCheckDbContext.Users.Remove(user);
                await _dailyCheckDbContext.SaveChangesAsync();
                return Ok(new { message = "User deleted successfully." });
            }
            if (googleUser != null)
                _dailyCheckDbContext.GoogleUsers.Remove(googleUser);
            await _dailyCheckDbContext.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
