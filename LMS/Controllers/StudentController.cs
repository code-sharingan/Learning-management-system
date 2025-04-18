using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;

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
            db.Database.GetDbConnection().Open();
            var classes = from e in db.Enrolls
                          join c in db.Classes on e.ClassId equals c.ClassId
                          join cr in db.Courses on c.CourseId equals cr.CourseId
                          where e.UId == uid
                          select new
                          {
                              subject = cr.Subject,
                              number = cr.Num,
                              name = cr.Name,
                              season = c.Season,
                              year = c.Year,
                              grade = string.IsNullOrEmpty(e.Grade) ? "--" : e.Grade
                          };
            
            return Json(classes);
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
            db.Database.GetDbConnection().Open();
            var assignments = from c in db.Classes join cr in db.Courses on c.CourseId equals cr.CourseId
                              join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                              join a in db.Assignments on ac.CategoryId equals a.CategoryId
                              join e in db.Enrolls on c.ClassId equals e.ClassId
                              where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year && e.UId == uid)
                              select new
                              {
                                  aname = a.Name,
                                  cname = ac.Name,
                                  due = a.Due,
                                  score = (from s in db.Submissions where s.UId == uid  && s.AssignmentId == a.AssignmentId
                                           select (int?)s.Score).FirstOrDefault()
                              };
            return Json(assignments);
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
            db.Database.GetDbConnection().Open();
            var trans = db.Database.GetDbConnection().BeginTransaction();
            db.Database.AutoTransactionsEnabled = false;
            db.Database.UseTransaction(trans);

            var assingment = (from a in db.Assignments join ac in db.AssignmentCategories on a.CategoryId equals ac.CategoryId
                        join c in db.Classes on ac.ClassId equals c.ClassId
                        join cr in db.Courses on c.CourseId equals cr.CourseId
                        where (a.Name == asgname && cr.Subject == subject && cr.Num == num && c.Year == year && c.Season == season && ac.Name == category)
                        select a).FirstOrDefault();
            if(assingment == null)
            {
                //that means there is no assignment of this type hence false;
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });

            }

            var submission = (from s in db.Submissions where s.AssignmentId == assingment.AssignmentId && s.UId == uid select s).FirstOrDefault();

            if(submission==null)
            {
                Submission ns = new();
                ns.UId = uid;
                ns.AssignmentId = assingment.AssignmentId;
                ns.Time = DateTime.Now;
                ns.Contents = contents;
                ns.Score = 0;

                db.Submissions.Add(ns);
                trans.Commit();
                db.Database.GetDbConnection().Close();
                return Json(new { success = true });

            }

            submission.Contents = contents;
            submission.Time = DateTime.Now;
            db.SaveChanges();
            trans.Commit();
            db.Database.GetDbConnection().Close();
            return Json(new { success = true });
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

            db.Database.GetDbConnection().Open();
            var trans = db.Database.GetDbConnection().BeginTransaction();
            db.Database.AutoTransactionsEnabled = false;
            db.Database.UseTransaction(trans);

            var cl = (from c in db.Classes
                      join cr in db.Courses on c.CourseId equals cr.CourseId
                      where (cr.Subject == subject && cr.Num == num && c.Year == year && c.Season == season)
                      select c).FirstOrDefault();
         
            if(cl == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });
            }

            var en = (from e in db.Enrolls where e.ClassId == cl.ClassId && e.UId == uid select e).FirstOrDefault();

            if(en == null)
            {
                Enroll e = new();
                e.UId = uid;
                e.ClassId = cl.ClassId;
                e.Grade = "--";

                db.Enrolls.Add(e);
                db.SaveChanges();
                trans.Commit();
                db.Database.GetDbConnection().Close();
                return Json(new { success = true });

            }

            trans.Rollback();
            db.Database.GetDbConnection().Close();
            return Json(new { success = false });
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
            return Json(null);
        }
                
        /*******End code to modify********/

    }
}

