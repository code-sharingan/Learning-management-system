using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            db.Database.GetDbConnection().Open();
            var clss = (from c in db.Classes
                       join cr in db.Courses on c.CourseId equals cr.CourseId
                       where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                       select c).FirstOrDefault();
            if(clss!=null)
            {
                var students = from e in db.Enrolls
                               join s in db.Students on e.UId equals s.UId
                               where e.ClassId == clss.ClassId
                               select new
                               {
                                   fname = s.FirstName,
                                   lname = s.LastName,
                                   uid = s.UId,
                                   dob = s.Dob,
                                   grade = e.Grade
                               };

                db.Database.GetDbConnection().Close();
                return Json(students.ToArray());
            }

            db.Database.GetDbConnection().Close();
            return Json(null);
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
            db.Database.GetDbConnection().Open();
            var assignments = from a in db.Assignments join ac in db.AssignmentCategories on a.CategoryId equals ac.CategoryId
                              join c in db.Classes on ac.ClassId equals c.ClassId join cr in db.Courses on c.CourseId equals cr.CourseId
                              where (ac.Name == category && cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                              select new
                              {
                                  aname = a.Name,
                                  cname = ac.Name,
                                  due = a.Due,
                                  submissions = (from s in db.Submissions where s.AssignmentId == a.AssignmentId select s).ToList().Count()
                              };

            db.Database.GetDbConnection().Close();
            return Json(assignments.ToArray());
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
            db.Database.GetDbConnection().Open();
            var acategories = (from ac in db.AssignmentCategories
                               join c in db.Classes on ac.ClassId equals c.ClassId
                               join cr in db.Courses on c.CourseId equals cr.CourseId
                               where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                               select ac).ToArray();

            db.Database.GetDbConnection().Close();
            return Json(acategories);
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
            db.Database.GetDbConnection().Open();
            var trans = db.Database.GetDbConnection().BeginTransaction();
            db.Database.AutoTransactionsEnabled = false;
            db.Database.UseTransaction(trans);
            var cls = (from c in db.Classes 
                             join cr in db.Courses on c.CourseId equals cr.CourseId
                             where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                             select c).FirstOrDefault();
            if(cls!=null)
            {
                    
                var acategory = (from ac in db.AssignmentCategories where ac.Name == category && ac.Weight == catweight && ac.ClassId == cls.ClassId select ac).FirstOrDefault();
                if (acategory == null)
                {
                    // that means there is no assignment category of this type and we can create one
                    AssignmentCategory ac = new();
                    ac.Name = category;
                    ac.Weight =(byte) catweight;
                    ac.ClassId = cls.ClassId;
                    db.AssignmentCategories.Add(ac);
                    db.SaveChanges();
                    trans.Commit();
                    db.Database.GetDbConnection().Close();
                    return Json(new { success = true });
                }

                trans.Rollback();
                return Json(new { success = false });

            }
            trans.Rollback();
            return Json(new { success = false });
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
            db.Database.GetDbConnection().Open();
            var trans = db.Database.GetDbConnection().BeginTransaction();
            db.Database.AutoTransactionsEnabled = false;
            db.Database.UseTransaction(trans);
            var cls = (from c in db.Classes
                       join cr in db.Courses on c.CourseId equals cr.CourseId
                       where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                       select c).FirstOrDefault();
            if (cls == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });
            }
            var aCat = (from ac in db.AssignmentCategories where ac.ClassId == cls.ClassId && ac.Name == category select ac).FirstOrDefault();
            if (aCat == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });
            }

            Assignment a = new();
            a.Name = asgname;
            a.Contents = asgcontents;
            a.Points = (uint)asgpoints;
            a.Due = asgdue;
            a.CategoryId = aCat.CategoryId;
            db.Assignments.Add(a);
            db.SaveChanges();
            trans.Commit();
            db.Database.GetDbConnection().Close();
            return Json(new { success = true });
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
            db.Database.GetDbConnection().Open();
            var cls = (from c in db.Classes
                       join cr in db.Courses on c.CourseId equals cr.CourseId
                       where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                       select c).FirstOrDefault();
            if (cls == null)
            { 
                db.Database.GetDbConnection().Close();
                return Json(null);
            }
            var aCat = (from ac in db.AssignmentCategories where ac.ClassId == cls.ClassId && ac.Name == category select ac).FirstOrDefault();
            if (aCat == null)
            { 
                db.Database.GetDbConnection().Close();
                return Json(null);
            }
            var assigment = (from a in db.Assignments where a.CategoryId == aCat.CategoryId && a.Name == asgname select a).First();
            var submissions = assigment.Submissions.ToArray();
            return Json(submissions);
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

            db.Database.GetDbConnection().Open();
            var trans = db.Database.GetDbConnection().BeginTransaction();
            db.Database.AutoTransactionsEnabled = false;
            db.Database.UseTransaction(trans);
            var cls = (from c in db.Classes
                       join cr in db.Courses on c.CourseId equals cr.CourseId
                       where (cr.Subject == subject && cr.Num == num && c.Season == season && c.Year == year)
                       select c).FirstOrDefault();
            if (cls == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });
            }
            var aCat = (from ac in db.AssignmentCategories where ac.ClassId == cls.ClassId && ac.Name == category select ac).FirstOrDefault();
            if (aCat == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });
            }
            var assigment = (from a in db.Assignments where a.CategoryId == aCat.CategoryId && a.Name == asgname select a).First();
            if(assigment == null)
            {
                trans.Rollback();
                db.Database.GetDbConnection().Close();
                return Json(new { success = false });

            }
            var submission = db.Submissions.First(s=>(s.UId == uid && s.AssignmentId== assigment.AssignmentId));
            submission.Score = (uint)score;
            db.SaveChanges();
            trans.Commit();
            db.Database.GetDbConnection().Close();
            return Json(new { success = true });
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
            var clss = (from c in db.Classes where c.Professor == uid select c).ToArray();

            return Json(clss);
        }


        
        /*******End code to modify********/
    }
}

