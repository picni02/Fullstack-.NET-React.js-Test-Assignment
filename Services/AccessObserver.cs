﻿using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Services
{
    public class AccessObserver
    {
        private readonly AppDbContext _dbContext;

        public AccessObserver(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CheckEventAccess(Event residentEvent)
        {
            // Provjera da li resident ima pristup ovom apartmanu
            bool hasAccess = _dbContext.ResidentApartments
                .Any(ra => ra.ResidentId == residentEvent.ResidentId && ra.ApartmentId == residentEvent.ApartmentId);

            //if (!hasAccess)
            //{
            //    throw new Exception($"Resident {residentEvent.ResidentId} attempted to {residentEvent.EventType} apartment {residentEvent.ApartmentId} without access.");
            //}

            return hasAccess;
        }

        public bool CheckResidentAccess(ResidentApartment residentApartment)
        {
            // Provjera da li resident već ima pristup ovom apartmanu
            bool exist = _dbContext.ResidentApartments
                .Any(ra => ra.ResidentId == residentApartment.ResidentId && ra.ApartmentId == residentApartment.ApartmentId);

            return exist;
        }
    }
}
