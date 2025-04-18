using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
			Department newDep = new();
			newDep.Name = name;
			newDep.Subject = subject;

			db.Database.GetDbConnection().Open();
			var trans = db.Database.GetDbConnection().BeginTransaction();
			db.Database.AutoTransactionsEnabled = false;
			db.Database.UseTransaction(trans);

			var query = from dep in db.Departments where subject == dep.Subject select dep;
			if (query.Count() == 0)
			{
				db.Departments.Add(newDep);
				db.SaveChanges();
				trans.Commit();
				return Json(new { success = true });
			} //else, department w/ same subject code already exists

			trans.Rollback();
			return Json(new { success = false });
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
			var query = from c in db.Courses where c.Subject == subject select new { number = c.Num, name = c.Name };

			return Json(query.ToArray());
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
			var query = from p in db.Professors where p.Subject == subject select new { lname = p.LastName, fname = p.FirstName, uid = p.UId };

			return Json(query.ToArray());

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
			Course newCourse = new();
			newCourse.Name = name;
			newCourse.Subject = subject;
			newCourse.Num = (short)number;

			db.Database.GetDbConnection().Open();
			var trans = db.Database.GetDbConnection().BeginTransaction();
			db.Database.AutoTransactionsEnabled = false;
			db.Database.UseTransaction(trans);

			var query = from c in db.Courses where subject == c.Subject && number == c.Num select c;
			if (query.Count() == 0)
			{
				db.Courses.Add(newCourse);
				db.SaveChanges();
				trans.Commit();
				return Json(new { success = true });
			} //else, course w/ same subject + number already exists

			trans.Rollback();
			return Json(new { success = false });
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
			Class newClass = new();
			newClass.Season = season;
			newClass.StartTime = TimeOnly.FromDateTime(start);
			newClass.EndTime = TimeOnly.FromDateTime(end);
			newClass.Year = (uint)year;
			newClass.Professor = instructor;
			newClass.Location = location;

			db.Database.GetDbConnection().Open();
			var trans = db.Database.GetDbConnection().BeginTransaction();
			db.Database.AutoTransactionsEnabled = false;
			db.Database.UseTransaction(trans);

			var course = from c in db.Courses where c.Subject == subject && c.Num == number select c;
			if (course.Count() == 0)
			{
				trans.Rollback();
				return Json(new { success = false });
			}
			newClass.Course = course.First();

			var conflict = from cl in db.Classes
						   where (year == cl.Year && season == cl.Season)
						   && (course == cl.Course // same course during same semester
								|| (location == cl.Location //same location and overlapping time window
									&& (cl.StartTime.CompareTo(newClass.StartTime) > 0 && cl.StartTime.CompareTo(newClass.EndTime) < 0)
										|| (cl.EndTime.CompareTo(newClass.StartTime) > 0 && cl.EndTime.CompareTo(newClass.EndTime) < 0))
						   )
						   select cl;
			if (conflict.Count() != 0)	
			{
				trans.Rollback();
				return Json(new { success = false });
			}

			db.Classes.Add(newClass);
			db.SaveChanges();
			trans.Commit();
			return Json(new { success = true });
		}


		/*******End code to modify********/

	}
}

