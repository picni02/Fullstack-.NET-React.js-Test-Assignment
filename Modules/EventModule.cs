using Microsoft.EntityFrameworkCore;
using Nancy;
using Nancy.ModelBinding;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using ResidentManagementSystem.Services;
using System;
using System.Collections.Generic;
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

                var events = _dbContext.Events.ToList()
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    EventTime = e.EventTime,
                    ResidentId = e.ResidentId,
                    EventType = e.EventType,
                    ApartmentId = e.ApartmentId,
                }).ToList();
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

            // POST add event
            Post("/", parameters =>
            {
                
                Event newEvent = this.Bind<Event>();

                var resident = _dbContext.Residents.Find(newEvent.ResidentId);
                var apartment = _dbContext.Apartments.Find(newEvent.ApartmentId);

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
                try
                {
                    
                    _accessObserver.CheckAccess(newEvent);

                    Console.WriteLine($"Event Time: {newEvent.EventTime}, Resident Id: {newEvent.ResidentId}, Event Type: {newEvent.EventType}, Apartment Id: {newEvent.ApartmentId}");

                    _dbContext.Events.Add(newEvent);

                    if (newEvent.EventType == "ENTRY")
                    {
                        resident.IsInside = true; // Ako je ENTRY, postavljanje IsInside na true (1)
                    }
                    else if (newEvent.EventType == "EXIT")
                    {
                        resident.IsInside = false; // Ako je EXIT, postavljanje IsInside na false (0)
                    }

                    _dbContext.SaveChanges();

                    return Response.AsJson(newEvent, HttpStatusCode.Created);
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
