using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public string Name { get; set; } = null!;
        public int AssignmentCategory { get; set; }
        public int ClassId { get; set; }
        public int AssignmentId { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime Due { get; set; }
        public uint Points { get; set; }

        public virtual AssignmentCategory AssignmentCategoryNavigation { get; set; } = null!;
        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
