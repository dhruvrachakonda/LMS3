using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from student in db.Students
                        join enrollment in db.Enrolleds on student.UId equals enrollment.UId
                        join offering in db.Classes on enrollment.ClassId equals offering.ClassId
                        join course in db.Courses on offering.CourseId equals course.CourseId
                        where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                        select new { fname = student.FName, lname = student.LName, uid = student.UId, dob = student.UId, enrollment.Grade };

            return Json(query);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
           
            //querying first the class
            var queryClass = from course in db.Courses
                             join offering in db.Classes on course.CourseId equals offering.CourseId
                             where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                             select offering.ClassId;

            int classID = queryClass.FirstOrDefault();

            
            if (category == null)
            {
                //and then the assignment
                 var query =from assignment in db.Assignments
                            join cat in db.AssignmentCategories on assignment.AssignmentCategory equals cat.AssignmentCategoryId
                            where assignment.ClassId == classID
                            select new { aname = assignment.Name, cname = cat.Name, due = assignment.Name, submissions = assignment.Submissions.Count};



                return Json(query);

            }
            
                var query1 = from assignment in db.Assignments
                            join cat in db.AssignmentCategories on assignment.AssignmentCategory equals cat.AssignmentCategoryId
                            where assignment.ClassId == classID && cat.Name == category
                            select new { aname = assignment.Name, cname = cat.Name, due = assignment.Name, submissions = assignment.Submissions.Count };



                return Json(query1);
            

           


        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {

            //this simple chunk of code has helped me to get the classID without remember the join so many times
            var queryClass = from course in db.Courses
                             join offering in db.Classes on course.CourseId equals offering.CourseId
                             where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                             select offering.ClassId;

            int classID = queryClass.FirstOrDefault();

            var queryCategories = from category in db.AssignmentCategories
                                  where category.ClassId == classID
                                  select new { name = category.Name, weight = category.Weight };

            return Json(queryCategories);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            try
            {
                var queryClass = from course in db.Courses
                                 join offering in db.Classes on course.CourseId equals offering.CourseId
                                 where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                                 select offering.ClassId;

                int classID = queryClass.FirstOrDefault();

                var queryCategory = from cat in db.AssignmentCategories
                                    where cat.Name == category && cat.ClassId == classID
                                    select cat;

                if (!queryCategory.Any())
                {
                    AssignmentCategory cat = new AssignmentCategory();
                    cat.Name = category;
                    cat.Weight = (uint)catweight;
                    cat.ClassId = classID;
                    db.AssignmentCategories.Add(cat);
                    db.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }

            
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            try
            {
                var queryClass = from course in db.Courses
                                 join offering in db.Classes on course.CourseId equals offering.CourseId
                                 where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                                 select offering.ClassId;

                int classID = queryClass.FirstOrDefault();

                var queryCategory = from cat in db.AssignmentCategories
                                    where cat.ClassId == classID && cat.Name == category
                                    select cat.AssignmentCategoryId;

                int catID = queryCategory.FirstOrDefault();

                Assignment a = new Assignment();
                a.Name = asgname;
                a.Points = (uint)asgpoints;
                a.Due = asgdue;
                a.AssignmentCategory = catID;
                a.ClassId = classID;
                a.Contents = asgcontents;

                db.Add(a);
                db.SaveChanges();

                var getStudents = (from enrollment in db.Enrolleds
                                   where enrollment.ClassId == classID
                                   select enrollment).ToList();

                foreach (Enrolled enrollment in getStudents)
                {
                    regradeStudent(enrollment.UId, classID);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new {success = false});
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var queryClass = from course in db.Courses
                             join offering in db.Classes on course.CourseId equals offering.CourseId
                             where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                             select offering.ClassId;

            int classID = queryClass.FirstOrDefault();

            var querySubmissions = from cat in db.AssignmentCategories
                                   join assignment in db.Assignments on cat.AssignmentCategoryId equals assignment.AssignmentCategory
                                   join submission in db.Submissions on assignment.AssignmentId equals submission.AssignmentId
                                   join student in db.Students on submission.UId equals student.UId
                                   where cat.Name == category && assignment.Name == asgname && submission.ClassId == classID
                                   select new { fname = student.FName, lname = student.LName, uid = student.UId, time = submission.Time, score = submission.Score };
                                   

            return Json(querySubmissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            try
            {
                var queryClass = from course in db.Courses
                                 join offering in db.Classes on course.CourseId equals offering.CourseId
                                 where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                                 select offering.ClassId;

                int classID = queryClass.FirstOrDefault();

                var querySubmissions = from cat in db.AssignmentCategories
                                       join assignment in db.Assignments on cat.AssignmentCategoryId equals assignment.AssignmentCategory
                                       join submission in db.Submissions on assignment.AssignmentId equals submission.AssignmentId
                                       join student in db.Students on submission.UId equals student.UId
                                       where cat.Name == category && assignment.Name == asgname & student.UId == uid
                                       select submission;

                foreach (var submission in querySubmissions)
                {
                    submission.Score = (uint)score;
                }

                db.SaveChanges();

                regradeStudent(uid, classID);
                return Json(new { success = true });

            }catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
           {

            var query = from course in db.Courses
                        join offering in db.Classes on course.CourseId equals offering.CourseId
                        where offering.Professor == uid
                        select new { subject = course.Subject, number = course.Num, name = course.Name , season = offering.SemesterSeason, year = offering.SemesterYear };

            return Json(query);

        }
        /// <summary>
        /// A helper method designed to regrade students everytime they are assigned an assignment or a new grade
        /// </summary>
        /// <param name="uid">student to be recalculated</param>
        /// <param name="classID">class they are to be recalculated in</param>
        private void regradeStudent(string uid, int classID)
        {
            var categories = (from category in db.AssignmentCategories
                        where category.ClassId == classID
                        select new {category.Name, category.Weight, category.Assignments}).ToList();

            double scaledCategoryTotal = 0;
            double weightsTotal = 0;

            foreach( var category in categories )
            {
                double categoryWeight = category.Weight;
                double totalPoints = 0;
                double earnedPoints = 0;
                double scaledTotal = 0;

                if(category.Assignments.Count == 0)
                {
                    continue;
                }

                weightsTotal += categoryWeight;

                var assignments = category.Assignments.ToList();

                foreach(var assignment in assignments)
                {
                    totalPoints += assignment.Points;
                    var assignmentID = assignment.AssignmentId;

                    var testIfSubmitted = from submission in db.Submissions
                                          where submission.AssignmentId == assignmentID && submission.UId == uid
                                          select submission.Score;

                    if (testIfSubmitted.Any())
                    {
                        earnedPoints += testIfSubmitted.FirstOrDefault();

                    }
                    else
                    {
                        earnedPoints += 0;
                    }


                }

                scaledTotal = (earnedPoints / totalPoints) * categoryWeight;
                scaledCategoryTotal += scaledTotal;


            }

            double scalingFactor = 100 / weightsTotal;
            double percentTotal = scaledCategoryTotal *  scalingFactor;
            string letterGrade = "";

            // Switch statement to determine the letter grade based on percentTotal
            switch (percentTotal)
            {
                case double grade when grade >= 93.0:
                    letterGrade = "A";
                    break;
                case double grade when grade >= 90.0:
                    letterGrade = "A-";
                    break;
                case double grade when grade >= 87.0:
                    letterGrade = "B+";
                    break;
                case double grade when grade >= 83.0:
                    letterGrade = "B";
                    break;
                case double grade when grade >= 80.0:
                    letterGrade = "B-";
                    break;
                case double grade when grade >= 77.0:
                    letterGrade = "C+";
                    break;
                case double grade when grade >= 73.0:
                    letterGrade = "C";
                    break;
                case double grade when grade >= 70.0:
                    letterGrade = "C-";
                    break;
                case double grade when grade >= 67.0:
                    letterGrade = "D+";
                    break;
                case double grade when grade >= 63.0:
                    letterGrade = "D";
                    break;
                case double grade when grade >= 60.0:
                    letterGrade = "D-";
                    break;
                default:
                    letterGrade = "E";
                    break;
            }

            var query = from e in db.Enrolleds
                        where e.UId == uid && e.ClassId == classID
                        select e;

            foreach(Enrolled e in query)
            {
                e.UId = uid; e.ClassId = classID; e.Grade = letterGrade;
            }

            db.SaveChanges();

        }
        /*******End code to modify********/
    }
}

