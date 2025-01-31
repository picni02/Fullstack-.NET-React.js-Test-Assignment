using Microsoft.EntityFrameworkCore;
using Nancy;
using Nancy.ModelBinding;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using ResidentManagementSystem.Services;

public class ResidentApartmentModule : NancyModule
{
    private readonly AppDbContext _dbContext;
    private readonly AccessObserver _accessObserver;

    public ResidentApartmentModule(AppDbContext dbContext, AccessObserver accessObserver) : base("/residentapartments")
    {
        _dbContext = dbContext;
        _accessObserver = accessObserver;

        //Get("/", _ => GetAllResidentsApartments());
        //Get("/residents/{residentId:int}/apartments/{apartmentId:int}", parameters => GetResidentApartment(parameters.residentId, parameters.apartmentId));
        //Post("/residents/{residentId:int}/apartments/{apartmentId:int}", parameters => AssignApartment(parameters.residentId, parameters.apartmentId));
        //Put("/residents/{residentId:int}/apartments/{apartmentId:int}", parameters => UpdateResidentApartment(parameters.residentId, parameters.apartmentId));
        //Delete("/residents/{residentId:int}/apartments/{apartmentId:int}", parameters => DeleteResidentApartment(parameters.residentId, parameters.apartmentId));

        // GET all events
        Get("/", _ =>
        {

            int page = Request.Query["page"].HasValue ? (int)Request.Query["page"] : 1;
            int pageSize = Request.Query["pageSize"].HasValue ? (int)Request.Query["pageSize"] : 20;

            // Ako page ili pageSize nisu validni brojevi, koristimo default
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var residentapartments = _dbContext.ResidentApartments
                .OrderBy(r => r.ResidentApartmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ra => new
                {
                    residentApartmentId = ra.ResidentApartmentId,
                    residentId = ra.ResidentId,
                    firstName = ra.Resident.FirstName,
                    lastName = ra.Resident.LastName,
                    apartmentId = ra.ApartmentId,
                    apartmentNumber = ra.Apartment.ApartmentNumber,
                    address = ra.Apartment.Address
                })
                .ToList();

            return Response.AsJson(residentapartments);
        });

        // GET a event by id
        Get("/{ResidentApartmentId:int}", parameters =>
        {
            int id = parameters.ResidentApartmentId;
            var getResidentApartment = _dbContext.ResidentApartments.Find(id);
            if (getResidentApartment == null)
                return HttpStatusCode.NotFound;

            return Response.AsJson(getResidentApartment);
        });

        Get("/search", parameters =>
        {
            string query = (string)Request.Query["query"] ?? "";

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

            var queryableEvents = _dbContext.ResidentApartments
                .Include(e => e.Resident)
                .Include(e => e.Apartment)
                .Where(e =>
                    e.ApartmentId.ToString().Contains(query) ||
                    e.ResidentId.ToString().Contains(query) ||
                    e.ResidentApartmentId.ToString().Contains(query) ||
                    (e.Resident != null && (e.Resident.FirstName.Contains(query) || e.Resident.LastName.Contains(query))) ||
                    (e.Apartment != null && (e.Apartment.ApartmentNumber.Contains(query) || e.Apartment.Address.Contains(query)))
                );


            var result = queryableEvents
                .OrderBy(e => e.ResidentApartmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    residentApartmentId = e.ResidentApartmentId,
                    residentId = e.ResidentId,
                    firstName = e.Resident != null ? e.Resident.FirstName : "N/A",
                    lastName = e.Resident != null ? e.Resident.LastName : "N/A",
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

            ResidentApartment newResidentApartment = this.Bind<ResidentApartment>();

            if (newResidentApartment == null)
            {
                return Response.AsJson(new { error = "Invalid resident-apartment data" }, HttpStatusCode.BadRequest);
            }

            var resident = _dbContext.Residents.Find(newResidentApartment.ResidentId);
            var apartment = _dbContext.Apartments.Find(newResidentApartment.ApartmentId);

            if (resident == null || apartment == null)
            {
                return Response.AsJson(new { error = "Resident or Apartment not found." }, HttpStatusCode.NotFound);
            }

            try
            {
                if (_accessObserver.CheckResidentAccess(newResidentApartment) == true)
                {
                    return Response.AsJson(new { error = "Resident is already assigned!" }, HttpStatusCode.Conflict);
                }
                Console.WriteLine($"Resident Id {newResidentApartment.ResidentId}, Apartment Id: {newResidentApartment.ApartmentId}");

                _dbContext.ResidentApartments.Add(newResidentApartment);

                _dbContext.SaveChanges();
                Console.WriteLine($"Resident: {resident.FirstName} {resident.LastName} is assigned to {apartment.ApartmentNumber}.");
                return Response.AsJson(new{
                    residentApartmentId = newResidentApartment.ResidentApartmentId,
                    residentId = newResidentApartment.ResidentId,
                    firstName = newResidentApartment.Resident.FirstName,
                    lastName = newResidentApartment.Resident.LastName,
                    apartmentId = newResidentApartment.ApartmentId,
                    apartmentNumber = newResidentApartment.Apartment.ApartmentNumber,
                    address = newResidentApartment.Apartment.Address
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
        Put("/{ResidentApartmentId:int}", parameters =>
        {
            int id = parameters.ResidentApartmentId;


            ResidentApartment existingRelation = _dbContext.ResidentApartments.FirstOrDefault(ra => ra.ResidentApartmentId == id);
            if (existingRelation == null)
                return HttpStatusCode.NotFound;


            var updatedRelation = this.Bind<ResidentApartment>();

            if (updatedRelation.ApartmentId > 0)
                existingRelation.ApartmentId = updatedRelation.ApartmentId;

            if (updatedRelation.ResidentId > 0)
                existingRelation.ResidentId = updatedRelation.ResidentId;

            _dbContext.SaveChanges();


            return Response.AsJson(existingRelation);
        });

        // DELETE event
        Delete("/{ResidentApartmentId:int}", parameters =>
        {
            int id = parameters.ResidentApartmentId;

            ResidentApartment relationToDelete = _dbContext.ResidentApartments.FirstOrDefault(ra => ra.ResidentApartmentId == id);
            if (relationToDelete == null)
                return HttpStatusCode.NotFound;

            _dbContext.ResidentApartments.Remove(relationToDelete);
            _dbContext.SaveChanges();
            return HttpStatusCode.NoContent;
        });
    }


    // GET: Retrieve all resident-apartment relationships
    private object GetAllResidentsApartments()
    {
        var allRelations = _dbContext.ResidentApartments
            .Select(ra => new
            {
                Resident = new
                {
                    ra.Resident.ResidentId,
                    ra.Resident.FirstName,
                    ra.Resident.LastName
                },
                Apartment = new
                {
                    ra.Apartment.ApartmentId,
                    ra.Apartment.ApartmentNumber,
                    ra.Apartment.Address
                }
            })
            .ToList();

        return Response.AsJson(allRelations);
    }

    // GET: Resident and Apartment details for a specific relationship
    private object GetResidentApartment(int residentId, int apartmentId)
    {
        var residentApartment = _dbContext.ResidentApartments
            .Where(ra => ra.ResidentId == residentId && ra.ApartmentId == apartmentId)
            .Select(ra => new
            {
                Resident = new
                {
                    ra.Resident.ResidentId,
                    ra.Resident.FirstName,
                    ra.Resident.LastName
                },
                Apartment = new
                {
                    ra.Apartment.ApartmentId,
                    ra.Apartment.ApartmentNumber,
                    ra.Apartment.Address
                }
            })
            .FirstOrDefault();

        if (residentApartment == null)
            return HttpStatusCode.NotFound;

        return Response.AsJson(residentApartment);
    }

    // POST: Assign a resident to an apartment
    private object AssignApartment(int residentId, int apartmentId)
    {
        var resident = _dbContext.Residents.FirstOrDefault(r => r.ResidentId == residentId);
        var apartment = _dbContext.Apartments.FirstOrDefault(a => a.ApartmentId == apartmentId);

        if (resident == null || apartment == null)
            return HttpStatusCode.NotFound;

        if (_dbContext.ResidentApartments.Any(ra => ra.ResidentId == residentId && ra.ApartmentId == apartmentId))
            return Response.AsJson(new { error = "Resident already assigned!" }, HttpStatusCode.Conflict); // Slučaj ako je već dodijeljen

        var residentApartment = new ResidentApartment
        {
            ResidentId = residentId,
            ApartmentId = apartmentId
        };

        _dbContext.ResidentApartments.Add(residentApartment);
        _dbContext.SaveChanges();

        return Response.AsJson(residentApartment, HttpStatusCode.Created);
    }

    // PUT: Update the resident-apartment relationship
    private object UpdateResidentApartment(int residentId, int apartmentId)
    {
        var existingRelation = _dbContext.ResidentApartments
            .FirstOrDefault(ra => (ra.ResidentId == residentId) && (ra.ApartmentId == apartmentId));

        if (existingRelation == null)
            return HttpStatusCode.NotFound;

        var apartment = _dbContext.Apartments.FirstOrDefault(a => a.ApartmentId == apartmentId);

        if (apartment == null)
            return HttpStatusCode.NotFound;

        existingRelation.ApartmentId = apartmentId;
        _dbContext.SaveChanges();

        return HttpStatusCode.OK;
    }

    // DELETE: Remove a resident from an apartment
    private object DeleteResidentApartment(int residentId, int apartmentId)
    {
        var residentApartment = _dbContext.ResidentApartments
            .FirstOrDefault(ra => ra.ResidentId == residentId && ra.ApartmentId == apartmentId);

        if (residentApartment == null)
            return HttpStatusCode.NotFound;

        _dbContext.ResidentApartments.Remove(residentApartment);
        _dbContext.SaveChanges();

        return HttpStatusCode.NoContent;
    }

}