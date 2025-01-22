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

                if (newResident.FirstName == null)
                    newResident.FirstName = string.Empty; 

                if (newResident.LastName == null)
                    newResident.LastName = string.Empty; 

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

                Resident resident = _dbContext.Residents.FirstOrDefault(r => r.ResidentId == id);
                if (resident == null)
                    return HttpStatusCode.NotFound;

                var updatedResident = this.Bind<Resident>();

                if (!string.IsNullOrEmpty(updatedResident.FirstName))
                    resident.FirstName = updatedResident.FirstName;

                if (!string.IsNullOrEmpty(updatedResident.LastName))
                    resident.LastName = updatedResident.LastName;

                if (updatedResident.IsInside != resident.IsInside)
                    resident.IsInside = updatedResident.IsInside;

                _dbContext.SaveChanges();

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
