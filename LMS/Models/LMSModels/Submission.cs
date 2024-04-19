using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int ClassId { get; set; }
        public string UId { get; set; } = null!;
        public string Contents { get; set; } = null!;
        public DateTime Time { get; set; }
        public uint Score { get; set; }

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Class Class { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
