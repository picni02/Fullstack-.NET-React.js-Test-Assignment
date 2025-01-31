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

                int page = Request.Query["page"].HasValue ? (int)Request.Query["page"] : 1;
                int pageSize = Request.Query["pageSize"].HasValue ? (int)Request.Query["pageSize"] : 20;

                // Ako page ili pageSize nisu validni brojevi, koristimo default
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                var residents = _dbContext.Residents
                    .OrderBy(r => r.ResidentId) 
                    .Skip((page - 1) * pageSize)  
                    .Take(pageSize) 
                    .Select(r => new
                    {
                        ResidentId = r.ResidentId,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        IsInside = r.IsInside
                    })
                    .ToList();

                return Response.AsJson(residents);
            });

            Get("/search", parameters =>
            {
                string query = (string)Request.Query["query"] ?? "";

                var result = _dbContext.Residents
                    .Where(r =>
                        r.FirstName.Contains(query) ||
                        r.LastName.Contains(query) ||
                        r.ResidentId.ToString().Contains(query)
                    )
                    .ToList();

                if (!result.Any())
                    return HttpStatusCode.NotFound;

                return Response.AsJson(result);
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

                if (string.IsNullOrWhiteSpace(newResident.FirstName) || string.IsNullOrWhiteSpace(newResident.LastName))
                {
                    return HttpStatusCode.BadRequest;
                }

                newResident.IsInside = newResident.IsInside ? true : false;

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
