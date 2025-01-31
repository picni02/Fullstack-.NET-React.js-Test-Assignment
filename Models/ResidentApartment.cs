using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ResidentManagementSystem.Models
{
    public class ResidentApartment
    {
        public int ResidentApartmentId { get; set; }
        public int ResidentId { get; set; }

        public virtual Resident Resident { get; set; }

        public int ApartmentId { get; set; }

        public virtual Apartment Apartment { get; set; }
    }
}
