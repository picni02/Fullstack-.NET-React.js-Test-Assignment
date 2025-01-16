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
        public EventModule(AppDbContext dbContext) : base("/events")
        {

            //Post("/", args =>
            //{
            //    var residentEvent = this.Bind<Event>();
            //    ElasticSearchService.IndexEvent(residentEvent);
            //    return Response.AsJson(residentEvent, HttpStatusCode.Created);
            //});

            //Get("/{ResidentId:int}", args =>
            //{
            //    List<Event> events = ElasticSearchService.GetEventsByResidentId(args.ResidentId);
            //    return Response.AsJson(events);
            //});

            //Delete("/{EventId:int}", args =>
            //{
            //    ElasticSearchService.DeleteEventById(args.EventId);
            //    return HttpStatusCode.NoContent;
            //});

            _dbContext = dbContext;

            // GET all residents
            Get("/", _ =>
            {

                var events = _dbContext.Events.ToList()
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    EventTime = e.EventTime,
                    ResidentId = e.ResidentId,
                    EventType = e.EventType,
                    ApartmentNumber = e.ApartmentNumber,
                }).ToList();
                return Response.AsJson(events);
            });

            // GET a resident by id
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

                // Provjeri ako su stringovi u redu
                if (newEvent.EventTime == default)
                    newEvent.EventTime = DateTime.Now; // Ako je null, postavi praznu vrijednost

                if (newEvent.EventType == null)
                    newEvent.EventType = "EXIT"; // Ako je null, postavi praznu vrijednost

                if (newEvent.ApartmentNumber == null)
                    newEvent.ApartmentNumber = "Unknown";

                Console.WriteLine($"Event Time: {newEvent.EventTime}, Resident Id: {newEvent.ResidentId}, Event Type: {newEvent.EventType}, Apartment Number: {newEvent.ApartmentNumber}");


                _dbContext.Events.Add(newEvent);
                _dbContext.SaveChanges();
                return Response.AsJson(newEvent, HttpStatusCode.Created);
            });

            // PUT update event
            Put("/{EventId:int}", parameters =>
            {
                int id = parameters.EventId;

                // Pronađi postojeći event u bazi
                Event existingEvent = _dbContext.Events.FirstOrDefault(e => e.EventId == id);
                if (existingEvent == null)
                    return HttpStatusCode.NotFound;

                // Bind dolaznih podataka iz JSON tijela
                var updatedEvent = this.Bind<Event>();

                // Ažuriraj samo one atribute koji nisu null ili zadani
                if (updatedEvent.EventTime != default)
                    existingEvent.EventTime = updatedEvent.EventTime;

                if (!string.IsNullOrEmpty(updatedEvent.EventType))
                    existingEvent.EventType = updatedEvent.EventType;

                if (!string.IsNullOrEmpty(updatedEvent.ApartmentNumber))
                    existingEvent.ApartmentNumber = updatedEvent.ApartmentNumber;

                if (updatedEvent.ResidentId > 0)
                    existingEvent.ResidentId = updatedEvent.ResidentId;

                _dbContext.SaveChanges();

                // Vrati ažurirani event
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
