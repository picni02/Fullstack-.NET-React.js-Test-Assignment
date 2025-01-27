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

                if (!apartmentsIds.Any())
                {
                    return Response.AsJson(new { error = "No apartments found for the given address." }, HttpStatusCode.NotFound);
                }

                var residentIds = _dbContext.ResidentApartments
                    .Where(ra => apartmentsIds.Contains(ra.ApartmentId))
                    .Select(ra => ra.ResidentId) 
                    .ToList();

                if (!residentIds.Any())
                {
                    return Response.AsJson(new { error = "No residents found for the given address." }, HttpStatusCode.NotFound);
                }

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
                    insidePercentage = totalResidents > 0 ? (int)Math.Round((double)insideCount / totalResidents * 100, MidpointRounding.AwayFromZero) : 0,
                    outsidePercentage = totalResidents > 0 ? (int)Math.Round((double)outsideCount / totalResidents * 100, MidpointRounding.AwayFromZero) : 0
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

                if (!topBuildings.Any())
                {
                    return Response.AsJson(new { error = "No events found in the database." }, HttpStatusCode.NotFound);
                }

                int totalEvents = topBuildings.Sum(b => b.EventCount);

                if (totalEvents == 0)
                {
                    return Response.AsJson(new { error = "Total event count is zero, cannot calculate percentages." }, HttpStatusCode.BadRequest);
                }

                var percentageList = topBuildings
                    .Select(b => new
                    {
                        b.Address,
                        b.EventCount,
                        RawPercentage = (double)b.EventCount / totalEvents * 100
                    })
                    .OrderByDescending(b => b.RawPercentage)
                    .ToList();

                var roundedPercentages = percentageList
                    .Select(b => new
                    {
                        b.Address,
                        b.EventCount,
                        SharePercentage = (int)Math.Round(b.RawPercentage, MidpointRounding.AwayFromZero)
                    })
                    .ToList();

                int totalRounded = roundedPercentages.Sum(b => b.SharePercentage);
                int difference = 100 - totalRounded;

                roundedPercentages[0] = new
                {
                    roundedPercentages[0].Address,
                    roundedPercentages[0].EventCount,
                    SharePercentage = roundedPercentages[0].SharePercentage + difference
                };

                return Response.AsJson(roundedPercentages);
            });
        }
    }
}
