using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Assignments = new HashSet<Assignment>();
            Enrolleds = new HashSet<Enrolled>();
            Submissions = new HashSet<Submission>();
        }

        public uint SemesterYear { get; set; }
        public string SemesterSeason { get; set; } = null!;
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Professor { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor ProfessorNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
