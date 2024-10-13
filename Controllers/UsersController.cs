using System;
using DailyCheckBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DailyCheckBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {

        private readonly DailyCheckDbContext _dailyCheckDbContext;

        public UsersController(DailyCheckDbContext dailyCheckDbContext)
        {
            _dailyCheckDbContext = dailyCheckDbContext;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _dailyCheckDbContext.GoogleUsers.ToListAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _dailyCheckDbContext.Users.FindAsync(id);

            if (user == null)
            {

                return NotFound();
            }

            return user;

        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> Add(User user)
        {
            _dailyCheckDbContext.Users.Add(user);
            await _dailyCheckDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _dailyCheckDbContext.Entry(user).State = EntityState.Modified;

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _dailyCheckDbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _dailyCheckDbContext.Users.Remove(user);
            await _dailyCheckDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _dailyCheckDbContext.Users.Any(e => e.Id == id);
        }
    }
}

