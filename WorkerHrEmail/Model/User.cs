using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerHrEmail.Model
{
    public class User
    {
        public int EmployeeId { set; get; }
        public string FirstNameRU { set; get; }
        public string Mail { set; get; }

        public DateTime? FirstDate { set; get; }

        public override string ToString()
        {
            return $"User: {EmployeeId}| {FirstNameRU}| {Mail}";
        }
    }
}
