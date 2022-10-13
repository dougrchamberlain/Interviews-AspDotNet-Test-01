using edu_services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace edu_services.Controllers
{
    //i wouldn't normally drop a class right in the controller, but for now I'm leaving it.
    public interface ISingleton
    {
        Dictionary<string, object> State { get; }
    }
    public class Singleton : ISingleton
    {
        public Dictionary<string, object> State { get; private set; }
        private static Singleton instance;

        public Singleton()
        {
            State = new Dictionary<string, object>();
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private ISingleton _singleton;
        private Classroom<Teacher, Student> classroom;

        public ApiController(ILogger<ApiController> logger, ISingleton singleton)
        {
            _logger = logger;
            _singleton = singleton;
            classroom = new Classroom<Teacher, Student>();
            if (!_singleton.State.ContainsKey("classroom"))
            {
                _singleton.State.Add("classroom", classroom);

            }

        }

        //TODO: Implement routes & domain using the Classroom object.

        [HttpGet("roster")]
        public ActionResult GetRoster()
        {

            try
            {
                var classroom = ((Classroom<Teacher, Student>)_singleton.State["classroom"]);

                var roster = classroom.GetRoster();

                return new JsonResult(new { Teacher = roster.Item1, Student = roster.Item2 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [HttpPost("teacher/{name}")]
        public ActionResult AddTeacher(string name)
        {
            try
            {
                var teacher = new Teacher { Name = name, Id = DateTime.Now.Ticks };
                var roster = (Classroom<Teacher, Student>)_singleton.State["classroom"];
                roster.AddTeacher(teacher);

                //save the system resources
#if DEBUG
                _logger.LogInformation(4224, null, $"{teacher.Name} : ADDED @{teacher.Id} : `/teacher/{teacher.Id}/`");
#endif
                // explicit values
                return Created($"/teacher/{teacher.Id}", teacher.Id); //expose as little as possible for privacy of data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex);
            }


        }

        [HttpPost("student/{name}")]
        public ActionResult AddStudent(string name)
        {
            try
            {
                var student = new Student { Name = name, Id = DateTime.Now.Ticks };
                var roster = (Classroom<Teacher, Student>)_singleton.State["classroom"];
                roster.AddStudent(student);

                //save the system resources
#if DEBUG
                //I demonstrated using variable interpolation with a literal prefix directive $
                _logger.LogInformation(8279, null, $"{student.Name} : ADDED @{student.Id} : `/student/{student.Id}/`");
#endif
                // explicit values
                return Created($"/student/{student.Id}/", student.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex);
            }



        }
    }
}
