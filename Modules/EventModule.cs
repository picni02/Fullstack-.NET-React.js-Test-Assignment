using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using Nancy;
using Nancy.ModelBinding;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using ResidentManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ResidentManagementSystem.Modules
{
    public class EventModule : NancyModule
    {
        private readonly AppDbContext _dbContext;
        private readonly AccessObserver _accessObserver;
        public EventModule(AppDbContext dbContext, AccessObserver accessObserver) : base("/events")
        {

            _dbContext = dbContext;
            _accessObserver = accessObserver;

            // GET all events
            Get("/", _ =>
            {

                int page = Request.Query["page"].HasValue ? (int)Request.Query["page"] : 1;
                int pageSize = Request.Query["pageSize"].HasValue ? (int)Request.Query["pageSize"] : 20;

                // Ako page ili pageSize nisu validni brojevi, koristimo default
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                var events = _dbContext.Events
                    .OrderByDescending(r => r.EventTime) 
                    .Skip((page - 1) * pageSize)  
                    .Take(pageSize)  
                    .Select(e => new
                    {
                        eventId = e.EventId,
                        eventTime = e.EventTime,
                        residentId = e.ResidentId,
                        firstName = e.Resident.FirstName,
                        lastName = e.Resident.LastName,
                        eventType = e.EventType,
                        apartmentId = e.ApartmentId,
                        apartmentNumber = e.Apartment.ApartmentNumber,
                        address = e.Apartment.Address,
                    })
                    .ToList();

                return Response.AsJson(events);
            });

            // GET a event by id
            Get("/{EventId:int}", parameters =>
            {
                int id = parameters.EventId;
                var getEvent = _dbContext.Events.Find(id);
                if (getEvent == null)
                    return HttpStatusCode.NotFound;

                return Response.AsJson(getEvent);
            });

            Get("/search", parameters =>
            {
                string query = (string)Request.Query["query"] ?? "";

              //  int page = Request.Query["page"].HasValue ? (int)Request.Query["page"] : 1;
                int page;
                if (!int.TryParse(Request.Query["page"], out page) || page < 1)
                {
                    page = 1;
                }

                int pageSize;
                if (!int.TryParse(Request.Query["pageSize"], out pageSize) || pageSize < 1)
                {
                    pageSize = 20;
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                var queryableEvents = _dbContext.Events
                    .Include(e => e.Resident)  
                    .Include(e => e.Apartment) 
                    .Where(e =>
                        e.ApartmentId.ToString().Contains(query) ||
                        e.ResidentId.ToString().Contains(query) ||
                        e.EventId.ToString().Contains(query) ||
                        (e.Resident != null && (e.Resident.FirstName.Contains(query) || e.Resident.LastName.Contains(query))) ||
                        (e.Apartment != null && (e.Apartment.ApartmentNumber.Contains(query) || e.Apartment.Address.Contains(query)))
                    );


                var result = queryableEvents
                    .OrderByDescending(e => e.EventTime) 
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        eventId = e.EventId,
                        eventTime = e.EventTime,
                        residentId = e.ResidentId,
                        firstName = e.Resident != null ? e.Resident.FirstName : "N/A",
                        lastName = e.Resident != null ? e.Resident.LastName : "N/A",
                        eventType = e.EventType,
                        apartmentId = e.ApartmentId,
                        apartmentNumber = e.Apartment != null ? e.Apartment.ApartmentNumber : "N/A",
                        address = e.Apartment != null ? e.Apartment.Address : "N/A",
                    })
                    .ToList();

                if (!result.Any())
                    return HttpStatusCode.NotFound;

                return Response.AsJson(new
                {
                    results = result
                });
            });


            // POST add event
            Post("/", parameters =>
            {

                Event newEvent = this.Bind<Event>();

                var resident = _dbContext.Residents.Find(newEvent.ResidentId);
                var apartment = _dbContext.Apartments.Find(newEvent.ApartmentId);
                newEvent.EventType.ToUpper();

                if (resident == null || apartment == null)
                {
                    return Response.AsJson(new { error = "Resident or Apartment not found." }, HttpStatusCode.NotFound);
                }

                if (newEvent == null)
                {
                    return Response.AsJson(new { error = "Invalid event data" }, HttpStatusCode.BadRequest);
                }

                
                if (newEvent.EventTime == default)
                        newEvent.EventTime = DateTime.Now;

                if (string.IsNullOrEmpty(newEvent.EventType))
                        newEvent.EventType = "EXIT";

                if (!_accessObserver.CheckEventAccess(newEvent))
                {
                    return Response.AsJson(new { error = "Access unauthorized!" }, HttpStatusCode.Unauthorized);
                }

                try
                {

                    Console.WriteLine($"Event Time: {newEvent.EventTime}, Resident Id: {newEvent.ResidentId}, Event Type: {newEvent.EventType}, Apartment Id: {newEvent.ApartmentId}");

                    _dbContext.Events.Add(newEvent);

                    if (newEvent.EventType.ToUpper() == "ENTRY")
                    {
                        resident.IsInside = true; // Ako je ENTRY, postavljanje IsInside na true (1)
                    }
                    else if (newEvent.EventType.ToUpper() == "EXIT")
                    {
                        resident.IsInside = false; // Ako je EXIT, postavljanje IsInside na false (0)
                    }

                    _dbContext.SaveChanges();
                    Console.WriteLine($"Resident: {resident.FirstName} {resident.LastName} changed status to {resident.IsInside}.");
                    return Response.AsJson(new {
                        eventId = newEvent.EventId,
                        eventTime = newEvent.EventTime,
                        residentId = newEvent.ResidentId,
                        firstName = newEvent.Resident.FirstName,
                        lastName = newEvent.Resident.LastName,
                        eventType = newEvent.EventType,
                        apartmentId = newEvent.ApartmentId,
                        apartmentNumber = newEvent.Apartment.ApartmentNumber,
                        address = newEvent.Apartment.Address
                    }, HttpStatusCode.Created);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    return Response.AsJson(new { error = ex.Message }, HttpStatusCode.Forbidden);
                }
            });


            // PUT update event
            Put("/{EventId:int}", parameters =>
            {
                int id = parameters.EventId;

              
                Event existingEvent = _dbContext.Events.FirstOrDefault(e => e.EventId == id);
                if (existingEvent == null)
                    return HttpStatusCode.NotFound;

               
                var updatedEvent = this.Bind<Event>();

                
                if (updatedEvent.EventTime != default)
                    existingEvent.EventTime = updatedEvent.EventTime;

                if (!string.IsNullOrEmpty(updatedEvent.EventType))
                    existingEvent.EventType = updatedEvent.EventType;

                if (updatedEvent.ApartmentId > 0)
                    existingEvent.ApartmentId = updatedEvent.ApartmentId;

                if (updatedEvent.ResidentId > 0)
                    existingEvent.ResidentId = updatedEvent.ResidentId;

                _dbContext.SaveChanges();

             
                return Response.AsJson(existingEvent);
            });

            // DELETE event
            Delete("/{EventId:int}", parameters =>
            {
                int id = parameters.EventId;

                Event eventToDelete = _dbContext.Events.FirstOrDefault(e => e.EventId == id);
                if (eventToDelete == null)
                    return HttpStatusCode.NotFound;

                _dbContext.Events.Remove(eventToDelete);
                _dbContext.SaveChanges();
                return HttpStatusCode.NoContent;
            });
        }

    }
}
