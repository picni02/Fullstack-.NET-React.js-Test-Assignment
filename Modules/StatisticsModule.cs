using Nancy;
using ResidentManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Modules
{
    public class StatisticsModule : NancyModule
    {
        private readonly AppDbContext _dbContext;

        public StatisticsModule(AppDbContext dbContext) : base("/statistics")
        {
            _dbContext = dbContext;

            Get("/residents-status", parameters =>
            {
                string address = (string)Request.Query["address"];

                var apartmentsIds = _dbContext.Apartments
                    .Where(a => a.Address == address)
                    .Select(a => a.ApartmentId)
                    .ToList();

                var residentIds = _dbContext.ResidentApartments
                    .Where(ra => apartmentsIds.Contains(ra.ApartmentId))
                    .Select(ra => ra.ResidentId) 
                    .ToList();

                var residents = _dbContext.Residents
                .Where(r => residentIds.Contains(r.ResidentId))
                .ToList();

                int totalResidents = residents.Count;
                int insideCount = residents.Count(r => r.IsInside);
                int outsideCount = totalResidents - insideCount;

                return Response.AsJson(new
                {
                    totalResidents,
                    insideCount,
                    outsideCount,
                    insidePercentage = totalResidents > 0 ? (int)((double)insideCount / totalResidents * 100) : 0,
                    outsidePercentage = totalResidents > 0 ? (int)((double)outsideCount / totalResidents * 100) : 0
                });
            });

            Get("/top-buildings", _ =>
            {
                var topBuildings = _dbContext.Events
                    .GroupBy(e => _dbContext.Apartments
                        .Where(a => a.ApartmentId == e.ApartmentId)
                        .Select(a => a.Address)
                        .FirstOrDefault()
                    )
                    .Select(g => new
                    {
                        Address = g.Key,
                        EventCount = g.Count()
                    })
                    .OrderByDescending(g => g.EventCount)
                    .Take(5)
                    .ToList();

                int totalEvents = topBuildings.Sum(b => b.EventCount);

                var response = topBuildings.Select(b => new
                {
                    b.Address,
                    b.EventCount,
                    SharePercentage = totalEvents > 0 ? (int)((double)b.EventCount / totalEvents * 100) : 0
                }).ToList();

                return Response.AsJson(response);
            });
        }
    }
}
