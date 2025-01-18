using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Models
{
    public class Apartment
    {
        public int ApartmentId { get; set; }
        public string ApartmentNumber { get; set; }
        public string Address { get; set; }


        public ICollection<ResidentApartment> ResidentApartments { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
