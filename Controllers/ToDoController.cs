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
            return Ok(new { todo = newToDo });
        }

        // PUT: api/todo/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateToDo([FromBody] ToDo todo)
        {
            Console.WriteLine("in the controller");
            var existingToDo = await _dailyCheckDbContext.ToDos.FindAsync(todo.Id);
            if (existingToDo == null) // Vérifie si l'ancien To-Do existe
            {
                Console.WriteLine("todo does not exist");

                return NotFound("The To-Do item was not found.");
            }
            if (!ModelState.IsValid)
            {
                Console.WriteLine("modelstate is not valid");

                return BadRequest(ModelState);
            }

            if (todo.Deadline < DateTime.Now)
            {
                Console.WriteLine("deadline can not be in the past");

                return BadRequest("The deadline cannot be in the past.");
            }

            existingToDo.Title = todo.Title;
            existingToDo.Description = todo.Description;
            existingToDo.Deadline = todo.Deadline;
            Console.WriteLine("todo uptodated");

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
                Console.WriteLine("todo uptodate saved");
                return Ok(new { todo = todo });
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("error");

                if (!ToDoExists(todo.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteToDo([FromBody] ToDo todo)
        {
            // Vérifie si l'ancien To-Do existe dans la base de données
            var existingToDo = await _dailyCheckDbContext.ToDos.FindAsync(todo.Id);
            if (existingToDo == null)
            {
                Console.WriteLine("The To-Do item was not found.");
                return NotFound("The To-Do item was not found.");
            }

            // Supprime l'élément To-Do
            _dailyCheckDbContext.ToDos.Remove(existingToDo);
            await _dailyCheckDbContext.SaveChangesAsync();

            Console.WriteLine($"To-Do with ID {todo.Id} deleted.");
            return Ok(new { message = $"To-Do \"{todo.Title}\" deleted successfully!" });
        }

        private bool ToDoExists(int id)
        {
            return _dailyCheckDbContext.ToDos.Any(e => e.Id == id);
        }
    }
}
