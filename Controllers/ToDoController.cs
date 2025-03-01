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
        private readonly ILogger<ToDoController> _logger;
        private readonly DailyCheckDbContext _dailyCheckDbContext;

        public ToDoController(
            DailyCheckDbContext dailyCheckDbContext,
            ILogger<ToDoController> logger
        )
        {
            _logger = logger;
            _dailyCheckDbContext = dailyCheckDbContext;
        }

        // GET: api/todo/all-todos
        [HttpPost("todos")]
        public async Task<ActionResult> GetToDos([FromBody] TodoRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            IQueryable<ToDo> query = _dailyCheckDbContext.ToDos.Where(todo =>
                todo.UserId == request.UserId
            //  && todo.Status == Status.Upcoming
            );
            IQueryable<ToDo> queryAll = _dailyCheckDbContext.ToDos.Where(todo =>
                todo.UserId == request.UserId
            );

            // if (request.UserStatus == Role.Admin)
            // {
            //     if (request.Status == Status.Upcoming || request.Status == null)
            //     {
            //         query = query.Where(todo =>
            //             todo.Status == Status.Upcoming && todo.UserId == request.UserId
            //         );
            //         queryAll = queryAll.Where(todo => todo.Status == Status.Upcoming);
            //     }
            //     else if (request.Status == Status.Done)
            //     {
            //         query = query.Where(todo =>
            //             todo.Status == request.Status.Value && todo.UserId == request.UserId
            //         );
            //         queryAll = queryAll.Where(todo => todo.Status == request.Status.Value);
            //     }
            //     else if (request.Status == Status.Cancelled)
            //     {
            //         query = query.Where(todo =>
            //             todo.Status == request.Status.Value && todo.UserId == request.UserId
            //         );
            //         queryAll = queryAll.Where(todo => todo.Status == request.Status.Value);
            //     }
            //     else if (request.Status == Status.Expired)
            //     {
            //         query = query.Where(todo =>
            //             todo.Status == request.Status.Value && todo.UserId == request.UserId
            //         );
            //         queryAll = queryAll.Where(todo => todo.Status == request.Status.Value);
            //     }
            //     else if (request.Status == Status.All)
            //     {
            //         query = _dailyCheckDbContext.ToDos.Where(todo => todo.UserId == request.UserId);
            //         queryAll = _dailyCheckDbContext.ToDos;
            //     }
            // }

            // else
            // {
            if (request.Status == Status.Upcoming || request.Status == null)
            {
                query = query.Where(todo =>
                    todo.Status == Status.Upcoming && todo.UserId == request.UserId
                );
            }
            else if (request.Status == Status.Done)
            {
                query = query.Where(todo =>
                    todo.Status == request.Status.Value && todo.UserId == request.UserId
                );
            }
            else if (request.Status == Status.Cancelled)
            {
                query = query.Where(todo =>
                    todo.Status == request.Status.Value && todo.UserId == request.UserId
                );
            }
            else if (request.Status == Status.All)
            {
                query = query.Where(todo => todo.UserId == request.UserId);
            }
            else if (request.Status == Status.Expired)
            {
                query = query.Where(todo =>
                    todo.Status == request.Status.Value && todo.UserId == request.UserId
                );
            }
            // }
            // Filtrer par statut si spécifié

            var usersTodos = await query.ToListAsync();
            foreach (var todo in usersTodos)
            {
                todo.Deadline = todo.Deadline.ToLocalTime();
            }
            var allUsersTodos = await queryAll.ToListAsync();

            // var todos = await queryAll.ToListAsync();

            // if (todos.Count == 0)
            // {
            //     return NotFound(new { message = $"No {request.Status} To-Dos found.", todos });
            // }
            // else if (usersTodos.Count == 0)
            // {
            //     return NotFound(new { message = $"No {request.Status} To-Dos found.", usersTodos });
            // }

            return Ok(new { usersTodos, usersTotalTodos = allUsersTodos.Count });
        }

        // POST: api/todo/create
        [HttpPost("create")]
        public async Task<ActionResult> CreateToDo([FromBody] ToDo todo)
        {
            Console.Write("this is the dtaa we got from the frontend");
            Console.WriteLine(todo.Deadline);

            if (todo == null)
            {
                return BadRequest("ToDo cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Console.WriteLine("todos deadline" + todo.Deadline.ToLocalTime());

            if (todo.Deadline.ToLocalTime() < DateTime.Now)
            {
                return BadRequest(new { message = "The deadline cannot be in the past." });
            }
            var newToDo = new ToDo
            {
                Title = todo.Title,
                Description = todo.Description,
                Status = Status.Upcoming,
                UserEmail = todo.UserEmail,
                UserId = todo.UserId,
                Deadline = todo.Deadline,
            };
            _dailyCheckDbContext.ToDos.Add(newToDo);
            await _dailyCheckDbContext.SaveChangesAsync();
            return Ok(new { todo = newToDo });
        }

        // PUT: api/todo/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateToDo([FromBody] ToDo todo)
        {
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

            if (todo.Status != Status.Expired && todo.Deadline < DateTime.Now)
            {
                Console.WriteLine("deadline can not be in the past");

                return BadRequest(new { message = "The deadline cannot be in the past." });
            }

            existingToDo.Title = todo.Title;
            existingToDo.Description = todo.Description;
            existingToDo.Deadline = todo.Deadline;

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
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
        [HttpPut("changestatus")]
        public async Task<IActionResult> ChangeToDosStatus([FromBody] ChangeStatusRequest request)
        {
            _logger.LogInformation("Received request to change To-Do status: {@Request}", request);

            var existingToDo = await _dailyCheckDbContext.ToDos.FindAsync(request.TodoId);

            if (existingToDo == null)
            {
                return NotFound("The To-Do item was not found.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: {@ModelState}", ModelState);
                return BadRequest(new { message = "Invalid data provided." });
            }

            if (existingToDo.Status == request.NewStatus)
            {
                return BadRequest(new { message = "To-Do item already has the specified status." });
            }

            existingToDo.Status = request.NewStatus;

            try
            {
                await _dailyCheckDbContext.SaveChangesAsync();
                _logger.LogInformation($"To-Do status changed to {request.NewStatus}");
                return Ok(
                    new { message = "To-Do status successfully changed.", todo = existingToDo }
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Concurrency error occurred while updating To-Do.");
                if (!ToDoExists(request.TodoId))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Relance l'exception pour être gérée par le middleware global
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
