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

        // GET: api/todo/all-todos
        [HttpPost("todos/all-todos")]
        public async Task<ActionResult> GetToDos([FromBody] string userId)
        {
            Console.WriteLine($"Received user ID: {userId}");

            if (userId == null || string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            var todos = await _dailyCheckDbContext
                .ToDos.Where(t => t.UserId == userId)
                .ToListAsync();

            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No To-Dos found.", todos = todos });
            }
            Console.WriteLine("response is ok");
            return Ok(new { todos = todos });
        }

        [HttpPost("todos/upcoming")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult> GetUpcomingToDos([FromBody] string userId)
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Upcoming && todo.UserId == userId) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste
            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No upcoming To-Dos found.", todos = todos }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
        }

        [HttpPost("todos/done")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult> GetDoneToDos([FromBody] string userId)
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Done && todo.UserId == userId) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste
            if (todos.Count() == 0)
            {
                Console.WriteLine("no done todos");
                return NotFound(new { message = "No done To-Dos found.", todos = todos }); // Réponse si vide
            }
            else
            {
                return Ok(new { todos = todos }); // Retourner la liste des To-Dos à venir
            }
        }

        [HttpPost("todos/cancelled")] // Point de terminaison pour obtenir les To-Dos à venir
        public async Task<ActionResult> GetCancelledToDos([FromBody] string userId)
        {
            // Récupérer tous les To-Dos avec le statut Upcoming
            var todos = await _dailyCheckDbContext
                .ToDos.Where(todo => todo.Status == Status.Cancelled && todo.UserId == userId) // Filtrer par statut
                .ToListAsync(); // Convertir le résultat en liste

            if (todos.Count() == 0)
            {
                return NotFound(new { message = "No cancelled To-Dos found.", todos = todos }); // Réponse si vide
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
                Console.WriteLine("the deadline is in the past" + todo.Deadline);
                return BadRequest(new { message = "The deadline cannot be in the past." });
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
        [HttpPut("todos/update")]
        public async Task<IActionResult> UpdateToDo([FromBody] ToDo todo)
        {
            Console.WriteLine("in the controller" + todo);
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

        // PUT: api/todo/cancel
        [HttpPut("cancel")]
        public async Task<IActionResult> ChangeToDosStatusToCancelled([FromBody] int todoId)
        {
            var existingToDo = await _dailyCheckDbContext.ToDos.FindAsync(todoId);
            if (existingToDo == null) // Vérifie si l'ancien To-Do existe
            {
                return NotFound("The To-Do item was not found.");
            }
            if (!ModelState.IsValid)
            {
                Console.WriteLine("modelstate is not valid");

                return BadRequest(ModelState);
            }
            if (existingToDo.Status == Status.Cancelled)
            {
                return BadRequest(new { message = "To-Do item already cancelled" });
            }

            existingToDo.Status = Status.Cancelled;

            Console.WriteLine("todo's status changed to cancelled");

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
                Console.WriteLine("todo cancelled");
                return Ok(new { todo = existingToDo });
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("error");

                if (!ToDoExists(todoId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // PUT: api/todo/done
        [HttpPut("done")]
        public async Task<IActionResult> ChangeToDosStatusToDone([FromBody] int todoId)
        {
            Console.WriteLine("in the controller");
            var existingToDo = await _dailyCheckDbContext.ToDos.FindAsync(todoId);
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
            if (existingToDo.Status == Status.Done)
            {
                return Ok(new { message = "To-Do item already done" });
            }

            existingToDo.Status = Status.Done;

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
                Console.WriteLine("todo's status changed to done");
                return Ok(new { todo = existingToDo });
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("error");

                if (!ToDoExists(todoId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("todos/delete")]
        public async Task<IActionResult> DeleteToDo([FromBody] ToDo todo)
        {
            Console.WriteLine("we're in the delete controller with:" + todo);
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
