using DailyCheckBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DailyCheckBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly DailyCheckDbContext _dailyCheckDbContext;

        public ToDoController(DailyCheckDbContext dailyCheckDbContext)
        {
            _dailyCheckDbContext = dailyCheckDbContext;
        }

        // GET: api/todo/todos
        [HttpGet("todos")]
      public async Task<ActionResult<IEnumerable<ToDo>>> GetToDos([FromQuery] Status? status)
        {
            if (status.HasValue) // Vérification si le statut est passé dans la requête
            {
                return await _dailyCheckDbContext.ToDos
                    .Where(t => t.Status == status.Value) // Filtrage par statut
                    .ToListAsync();
            }
            return await _dailyCheckDbContext.ToDos.ToListAsync();
        }


        // GET: api/todo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDo>> GetToDoById(int id)
        {
            var todo = await _dailyCheckDbContext.ToDos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return todo;
        }

        // POST: api/todo/create
        [HttpPost("create")]
        public async Task<ActionResult<ToDo>> CreateToDo([FromBody] ToDo todo)
        {
            if (todo == null)
            {
                return BadRequest("ToDo cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dailyCheckDbContext.ToDos.Add(todo);
            await _dailyCheckDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetToDoById), new { id = todo.Id }, todo);
        }

        // PUT: api/todo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDo(int id, [FromBody] ToDo todo)
        {
            if (id != todo.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dailyCheckDbContext.Entry(todo).State = EntityState.Modified;

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoExists(id))
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

        // DELETE: api/todo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDo(int id)
        {
            var todo = await _dailyCheckDbContext.ToDos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _dailyCheckDbContext.ToDos.Remove(todo);
            await _dailyCheckDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ToDoExists(int id)
        {
            return _dailyCheckDbContext.ToDos.Any(e => e.Id == id);
        }
    }
}
