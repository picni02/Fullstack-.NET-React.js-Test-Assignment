using Nancy;
using Nancy.ModelBinding;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Modules
{
    public class ApartmentModule : NancyModule
    {
        private readonly AppDbContext _dbContext;

        public ApartmentModule(AppDbContext dbContext) : base("/apartments")
        {
            _dbContext = dbContext;

            // GET all apartments
            Get("/", _ =>
            {

                var apartments = _dbContext.Apartments.ToList()
                .Select(a => new Apartment
                {
                    ApartmentId = a.ApartmentId,
                    ApartmentNumber = a.ApartmentNumber,
                    Address = a.Address
                }).ToList();
                return Response.AsJson(apartments);
            });

            // GET a apartment by id
            Get("/{ApartmentId:int}", parameters =>
            {
                int id = parameters.ApartmentId;
                var getApartment = _dbContext.Apartments.Find(id);
                if (getApartment == null)
                    return HttpStatusCode.NotFound;

                return Response.AsJson(getApartment);
            });

            // POST add apartment
            Post("/", parameters =>
            {

                Apartment newApartment = this.Bind<Apartment>();

                // Provjeri ako su stringovi u redu
                if (newApartment.ApartmentNumber == null)
                    newApartment.ApartmentNumber = "A100"; // Ako je null, postavi praznu vrijednost

                if (newApartment.Address == null)
                    newApartment.Address = "street"; // Ako je null, postavi praznu vrijednost

                Console.WriteLine($"Apartment Number: {newApartment.ApartmentNumber}, Address: {newApartment.Address}");


                _dbContext.Apartments.Add(newApartment);
                _dbContext.SaveChanges();
                return Response.AsJson(newApartment, HttpStatusCode.Created);
            });

            // PUT update apartment
            Put("/{ApartmentId:int}", parameters =>
            {
                int id = parameters.ApartmentId;

                // Pronađi postojeći apartment u bazi
                Apartment existingApartment = _dbContext.Apartments.FirstOrDefault(a => a.ApartmentId == id);
                if (existingApartment == null)
                    return HttpStatusCode.NotFound;

                // Bind dolaznih podataka iz JSON tijela
                var updatedApartment = this.Bind<Apartment>();

                // Ažuriraj samo one atribute koji nisu null ili zadani
                if (!string.IsNullOrEmpty(updatedApartment.ApartmentNumber))
                    existingApartment.Address = updatedApartment.Address;

                if (!string.IsNullOrEmpty(updatedApartment.Address))
                    existingApartment.Address = updatedApartment.Address;

                _dbContext.SaveChanges();

                // Vrati ažurirani event
                return Response.AsJson(existingApartment);
            });

            // DELETE apartment
            Delete("/{ApartmentId:int}", parameters =>
            {
                int id = parameters.ApartmentId;

                Apartment apartmentToDelete = _dbContext.Apartments.FirstOrDefault(a => a.ApartmentId == id);
                if (apartmentToDelete == null)
                    return HttpStatusCode.NotFound;

                _dbContext.Apartments.Remove(apartmentToDelete);
                _dbContext.SaveChanges();
                return HttpStatusCode.NoContent;
            });
        }
    }
}
