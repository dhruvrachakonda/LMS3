using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            //query to retrieve enrollments based on uid
            var query = from student in db.Students
                        join enrollment in db.Enrolleds on student.UId equals enrollment.UId
                        join offering in db.Classes on enrollment.ClassId equals offering.ClassId
                        join courses in db.Courses on offering.CourseId equals courses.CourseId
                        into rightSide
                        from j in rightSide.DefaultIfEmpty()
                        where student.UId == uid
                        select new
                        {
                            subject = j.Subject,
                            number = j.Num,
                            name = j.Name,
                            season = offering.SemesterSeason,
                            year = offering.SemesterYear,
                            grade = enrollment.Grade
                        };

            return Json(query);

        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {

            //we are using the unusual left join on double parameters here
            var query1 = from assignment in db.Assignments
                         join offering in db.Classes on assignment.ClassId equals offering.ClassId
                         join course in db.Courses on offering.CourseId equals course.CourseId
                         join category in db.AssignmentCategories on assignment.AssignmentCategory equals category.AssignmentCategoryId
                         where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                         select new { assignment, category };

            var query2 = from q in query1  // query1 holds the assignments for the class
                         join s in db.Submissions
                         on new { A = q.assignment.AssignmentId, B = uid } equals new { A = s.AssignmentId, B = s.UId }
                         into joined
                         from j in joined.DefaultIfEmpty()

                         select new{aname = q.assignment.Name, cname = q.category.Name, due = q.assignment.Due, score = (j == null ? null : (uint?)j.Score)};

            return Json(query2);

        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {

            try
            {
                var assnClass = from offering in db.Classes
                                join course in db.Courses on offering.CourseId equals course.CourseId
                                where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                                select offering.ClassId;

                int classID = assnClass.FirstOrDefault();

                var cat = from categ in db.AssignmentCategories
                          where categ.Name == category && categ.ClassId == classID
                          select categ.AssignmentCategoryId;

                int assnCatID = cat.FirstOrDefault();

                var assn = from assignment in db.Assignments
                           where assignment.ClassId == classID && assignment.AssignmentCategory == assnCatID
                           select assignment.AssignmentId;

                int assnID = assn.FirstOrDefault();

                var submission = from sub in db.Submissions
                                 where sub.AssignmentId == assnID && sub.ClassId == classID && sub.UId == uid
                                 select sub;

                int nextSubID = (int)db.Submissions.Max(id => id.SubmissionId) + 1;

                if (submission.Any())
                {
                    foreach (Submission sub in submission)
                    {
                        sub.Contents = contents;
                        sub.Time = DateTime.Now;
                    }

                    db.SaveChanges();
                }
                else
                {
                    Submission s = new Submission();
                    s.AssignmentId = assnID;
                    s.ClassId = classID;
                    s.UId = uid;
                    s.Contents = contents;
                    s.Time = DateTime.Now;
                    s.Score = 0;

                    db.Submissions.Add(s);

                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
           
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var classQuery = from course in db.Courses
                             join offering in db.Classes on course.CourseId equals offering.CourseId
                             where course.Subject == subject && course.Num == num && offering.SemesterSeason == season && offering.SemesterYear == year
                             select offering.ClassId;

            int classID = classQuery.FirstOrDefault();

            var existanceQuery = from enrollment in db.Enrolleds
                                 where enrollment.UId == uid && enrollment.ClassId ==classID
                                 select enrollment;

            if(existanceQuery.Any())
            {
                return Json(new { success = false });
            }

            Enrolled e = new Enrolled();
            e.UId = uid;
            e.ClassId = classID;
            e.Grade = "--";

            db.Add(e);
            db.SaveChanges();

            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var gpaQuery = from enrollment in db.Enrolleds
                           where enrollment.UId == uid
                           select enrollment.Grade;

            double gpa = 0;

            double totalPoints = 0;
            int totalClasses = 0;

            foreach (var grade in gpaQuery)
            {
                if (grade == "A")
                {
                    totalClasses++;
                    totalPoints += 4.0;
                } else if (grade == "A-")
                {
                    totalClasses++;
                    totalPoints += 3.7;
                } else if (grade == "B+")
                {
                    totalClasses++;
                    totalPoints += 3.3;

                } else if (grade == "B")
                {
                    totalClasses++;
                    totalPoints += 3.0;
                } else if (grade == "B-")
                {
                    totalClasses++;
                    totalPoints += 2.7;
                } else if (grade == "C+")
                {
                    totalClasses++;
                    totalPoints += 2.3;
                } else if (grade == "C")
                {
                    totalClasses++;
                    totalPoints += 2.0;
                } else if(grade == "C-")
                {
                    totalClasses++;
                    totalPoints += 1.7;
                } else if (grade == "D+")
                {
                    totalClasses++;
                    totalPoints += 1.3;
                } else if(grade == "D")
                {
                    totalClasses++;
                    totalPoints += 1.0;
                }
                else if (grade == "D-")
                {
                    totalClasses++;
                    totalPoints += 0.7;
                }
                else if (grade == "E")
                {
                    totalClasses++;
                    totalPoints += 0;
                }else if (grade == "--")
                {
                    totalPoints += 0;
                }


                if(totalClasses == 0)
                {
                    gpa = 0;
                }
                else
                {
                    gpa = totalPoints / totalClasses;
                }


            }

            return Json(new {gpa = gpa});
        }
                
        /*******End code to modify********/

    }
}

