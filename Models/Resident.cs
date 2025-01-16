using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResidentManagementSystem.Models
{
    public class Resident
    {
        public int ResidentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsInside { get; set; }
    }

    public class ResidentDB
    {
        public int ResidentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int IsInside { get; set; }
    }
}
