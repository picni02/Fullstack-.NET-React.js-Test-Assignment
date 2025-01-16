using Nancy;
using Nancy.ModelBinding;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResidentManagementSystem.Modules
{
    public class ResidentModule : NancyModule
    {
        private readonly AppDbContext _dbContext;

        public ResidentModule(AppDbContext dbContext) : base("/residents")
        {
            _dbContext = dbContext;

            // GET all residents
            Get("/", _ =>
            {
                
                var residents = _dbContext.Residents.ToList()
                .Select(r => new Resident {
                    ResidentId = r.ResidentId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    IsInside = r.IsInside,
                }).ToList();
                return Response.AsJson(residents);
            });

            // GET a resident by id
            Get("/{ResidentId:int}", parameters =>
            {
                int id = parameters.ResidentId;
                var resident = _dbContext.Residents.Find(id);
                if (resident == null)
                    return HttpStatusCode.NotFound;

                return Response.AsJson(resident);
            });

            // POST add resident
            Post("/", parameters =>
            {

                Resident newResident = this.Bind<Resident>();

                // Provjeri ako su stringovi u redu
                if (newResident.FirstName == null)
                    newResident.FirstName = string.Empty; // Ako je null, postavi praznu vrijednost

                if (newResident.LastName == null)
                    newResident.LastName = string.Empty; // Ako je null, postavi praznu vrijednost

                // Provjeri i konvertuj bool vrijednost
                if (parameters.IsInside != null)
                    newResident.IsInside = Convert.ToBoolean(parameters.IsInside);

                Console.WriteLine($"FirstName: {newResident.FirstName}, LastName: {newResident.LastName}, IsInside: {newResident.IsInside}");


                _dbContext.Residents.Add(newResident);
                _dbContext.SaveChanges();
                return Response.AsJson(newResident, HttpStatusCode.Created);
            });

            // PUT update resident
            Put("/{ResidentId:int}", parameters =>
            {
                int id = parameters.ResidentId;

                // Pronađi postojećeg rezidenta u bazi
                Resident resident = _dbContext.Residents.FirstOrDefault(r => r.ResidentId == id);
                if (resident == null)
                    return HttpStatusCode.NotFound;

                // Bind dolaznih podataka iz JSON tijela
                var updatedResident = this.Bind<Resident>();

                // Ažuriraj samo one atribute koji nisu null
                if (!string.IsNullOrEmpty(updatedResident.FirstName))
                    resident.FirstName = updatedResident.FirstName;

                if (!string.IsNullOrEmpty(updatedResident.LastName))
                    resident.LastName = updatedResident.LastName;

                // Ažuriraj IsInside samo ako je proslijeđen u JSON-u
                if (updatedResident.IsInside != resident.IsInside)
                    resident.IsInside = updatedResident.IsInside;

                // Spremi promjene u bazi
                _dbContext.SaveChanges();

                // Vrati ažuriranog rezidenta
                return Response.AsJson(resident);
            });

            // DELETE resident
            Delete("/{ResidentId:int}", parameters =>
            {
                Resident deleteResident = this.Bind<Resident>();
                Resident resident = _dbContext.Residents.Find(deleteResident.ResidentId);
                if (resident == null)
                    return HttpStatusCode.NotFound;

                _dbContext.Residents.Remove(resident);
                _dbContext.SaveChanges();
                return HttpStatusCode.NoContent;
            });


        }
    }
}
