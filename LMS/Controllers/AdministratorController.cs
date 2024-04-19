using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            try
            {
                //we'll first see if the department already exists
                var query = from department in db.Departments
                            where department.Subject == subject
                            select department; 

                //if it doesnt, we'll add it
                if (!query.Any())
                {
                    Department d = new Department();
                    d.Name = name;
                    d.Subject = subject;

                    db.Departments.Add(d);
                    db.SaveChanges();
                    return Json(new { success = true });

                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from course in db.Courses
                        where course.Subject == subject
                        select new {number = course.Num, name = course.Name};
          
          return Json(query);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from professor in db.Professors
                        where professor.WorksIn == subject
                        select new { lname = professor.LName, fname = professor.FName, uid = professor.UId };
            
            return Json(query);
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            try
            {
                var courseExistance = from crs in db.Courses
                                      where crs.Subject == subject && crs.Num == number && crs.Name == name
                                      select crs;

                if (courseExistance.Any())
                {
                    return Json(new { success = false });
                }

                Course course = new Course();
                course.Subject = subject;
                course.Name = name;
                course.Num = number;

                db.Courses.Add(course);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });

            }

        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            try
            {
                var queryCourse = from course in db.Courses
                                  where course.Subject == subject && course.Num == number
                                  select course.CourseId;


                int courseID = queryCourse.FirstOrDefault();
                
                //to see if the class exists already
                var classExistance = from offering in db.Classes
                                     where offering.Location == location
                                     select offering;

                if (classExistance.Any())
                {
                    return Json(new { success = false });
                }



                Class cls = new Class();

                TimeOnly timeOnly = new TimeOnly(10, 10, 10);


                cls.SemesterSeason = season;
                cls.SemesterYear = (uint)year;
                cls.CourseId = courseID;
                cls.Location = location;
                cls.StartTime = TimeOnly.FromDateTime(start);
                cls.EndTime = TimeOnly.FromDateTime(end);
                cls.Professor = instructor;

                db.Add(cls);

                db.SaveChanges();


                return Json(new { success = true });



            }
            catch
            {
                return Json(new { success = false });
            }


         }
        /*******End code to modify********/

    }
}

