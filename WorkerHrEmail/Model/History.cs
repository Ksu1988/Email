using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerHrEmail.Model
{
    public class History
    {
        public int EmployeeId { set; get; }
        public DateTime? WellcomeEmail { set; get; }
        public DateTime? OneYearEmail { set; get; }
        public DateTime? Report { set; get; }
        public int Diff1 { set; get; }
        public int Diff2 { set; get; }
        public string LastNameRu { set; get; }
        public string FirstNameRu { set; get; }
        public string MiddleNameRu { set; get; }
        public string Mail { set; get; }
    }
}

