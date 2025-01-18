using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResidentManagementSystem.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public DateTime EventTime { get; set; }
        public int ResidentId { get; set; }
        public string EventType { get; set; } // Entry/Exit
        public int ApartmentId { get; set; }

    }
}
