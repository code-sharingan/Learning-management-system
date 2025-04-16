using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolls = new HashSet<Enroll>();
        }

        public uint Year { get; set; }
        public string Season { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string Professor { get; set; } = null!;
        public string Location { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor ProfessorNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enroll> Enrolls { get; set; }
    }
}
