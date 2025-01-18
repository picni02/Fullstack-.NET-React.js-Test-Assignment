using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Models
{
    public class ResidentApartment
    {
        public int ResidentId { get; set; }
        public Resident Resident { get; set; }

        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; }
    }
}
