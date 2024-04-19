using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using NuGet.Frameworks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from department in db.Departments
                        select department;

            JsonResult jsonQuery = Json(query.ToArray());

            return jsonQuery;
        }


        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog = from d in db.Departments
                          select new
                          {
                              subject = d.Subject,
                              dname = d.Name,
                              courses = (from c in db.Courses
                                         where c.Subject == d.Subject
                                         select new
                                         {
                                             number = c.Num,
                                             cname = c.Name
                                         }).ToArray()
                          };

            JsonResult jsonQuery = Json(catalog);

            return jsonQuery;
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var offerings = from cor in db.Courses
                            where cor.Num == number && cor.Subject == subject
                            join offr in db.Classes on cor.CourseId equals offr.CourseId
                            join professor in db.Professors on offr.Professor equals professor.UId
                            select new
                            {
                                season = offr.SemesterSeason,
                                year = offr.SemesterYear,
                                location = offr.Location,
                                start = offr.StartTime,
                                end = offr.EndTime,
                                fname = professor.FName,
                                lname = professor.LName
                            };

            JsonResult jsonQuery = Json(offerings);
            return jsonQuery;


        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var assignment = from cor in db.Courses
                             join offr in db.Classes on cor.CourseId equals offr.CourseId
                             join assn in db.Assignments on offr.ClassId equals assn.ClassId
                             join cat in db.AssignmentCategories on assn.AssignmentCategory equals cat.AssignmentCategoryId
                             where cor.Subject == subject && cor.Num == num &&
                             offr.SemesterSeason == season && offr.SemesterYear == year &&
                             cat.Name == category && assn.Name == asgname
                             select assn.Contents;


            string contents = "";
            foreach (var item in assignment)
            {
                contents = item;
            }
                            
            return Content(contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var classIDQuery = from course in db.Courses
                          join offering in db.Classes on course.CourseId equals offering.CourseId
                          where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                          select offering.ClassId;

            int classID = classIDQuery.FirstOrDefault();

            var categoryQuery = from course in db.Courses
                                join offering in db.Classes on course.CourseId equals offering.CourseId
                                join cat in db.AssignmentCategories on offering.ClassId equals cat.ClassId
                                where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year && cat.Name == category
                                select cat.AssignmentCategoryId;

            int assignmentCategoryId = categoryQuery.FirstOrDefault();   


            var assignmentQuery = from assignment in db.Assignments
                                  where assignment.Name == asgname && assignment.AssignmentCategory == assignmentCategoryId && assignment.ClassId == classID
                                  select assignment.AssignmentId;

            int assignmentID = assignmentQuery.FirstOrDefault();

            var submissionQuery = from submission in db.Submissions
                                  where submission.AssignmentId == assignmentID && submission.ClassId == classID && submission.UId == uid
                                  select submission.Contents;

            string contents = "";

            foreach (var submission in submissionQuery)
            {
                contents += submission;
            }

                return Content(contents);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            try
            {
                var student = from stu in db.Students
                              where stu.UId == uid
                              select new { fname = stu.FName, lname = stu.LName, uid = stu.UId, department = stu.Major };

                var professor = from prof in db.Professors
                                where prof.UId == uid
                                select new { fname = prof.FName, lname = prof.LName, uid = prof.UId, department = prof.WorksIn };

                var admin = from ad in db.Administrators
                            where ad.UId == uid
                            select new { fname = ad.FName, lname = ad.LName, uid = ad.UId };

                if (student.Any())
                {
                    return Json(student.First());
                }
                else if (professor.Any())
                {
                    return Json(professor.First());
                }
                else if (admin.Any())
                {
                    return Json(admin.First());
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


        /*******End code to modify********/
    }
}

