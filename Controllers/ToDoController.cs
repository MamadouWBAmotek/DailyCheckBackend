using System;
using DailyCheckBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<ToDo>>> GetToDos()
        {
            var todos = await _dailyCheckDbContext.ToDos.ToListAsync();
            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No To-Dos found." }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
        }

        [HttpGet("todos/upcoming")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult<IEnumerable<ToDo>>> GetUpcomingToDos()
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Upcoming) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste
            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No upco;ingu; To-Dos found." }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
        }

        [HttpGet("todos/done")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult<IEnumerable<ToDo>>> GetDoneToDos()
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Done) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste
            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No done To-Dos found." }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
        }

        [HttpGet("todos/cancelled")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult<IEnumerable<ToDo>>> GetCancelledToDos()
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Cancelled) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste

            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No cancelled To-Dos found." }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
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
        public async Task<ActionResult> CreateToDo([FromBody] ToDo todo)
        {
            Console.WriteLine(
                $"Received: {todo.Title}, {todo.Description}, {todo.Deadline}, {todo.UserId}"
            );

            if (todo == null)
            {
                return BadRequest("ToDo cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (todo.Deadline < DateTime.Now)
            {
                return BadRequest("The deadline cannot be in the past.");
            }
            var newToDo = new ToDo
            {
                Title = todo.Title,
                Description = todo.Description,
                Status = Status.Upcoming, // Default status if not provided
                UserId = todo.UserId,
                Deadline = todo.Deadline,
            };

            _dailyCheckDbContext.ToDos.Add(newToDo);
            await _dailyCheckDbContext.SaveChangesAsync();
            Console.WriteLine("this is the controller");
            return Ok();
        }

        // PUT: api/todo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDo(int id, [FromBody] ToDo todo)
        {
            if (id != todo.Id)
            {
                return BadRequest("The ID provided does not match the ToDo item.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (todo.Deadline < DateTime.Now)
            {
                return BadRequest("The deadline cannot be in the past.");
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
