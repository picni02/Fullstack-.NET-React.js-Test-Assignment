using Nancy;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;

public class ResidentApartmentModule : NancyModule
{
    private readonly AppDbContext _dbContext;

    public ResidentApartmentModule(AppDbContext dbContext) : base("/residentapartments")
    {
        _dbContext = dbContext;

        Get("/", _ => GetAllResidentsApartments());
        Get("/residents/{residentId}/apartments/{apartmentId}", parameters => GetResidentApartment(parameters.residentId, parameters.apartmentId));
        Post("/residents/{residentId}/apartments/{apartmentId}", parameters => AssignApartment(parameters.residentId, parameters.apartmentId));
        Put("/residents/{residentId}/apartments/{apartmentId}", parameters => UpdateResidentApartment(parameters.residentId, parameters.apartmentId));
        Delete("/residents/{residentId}/apartments/{apartmentId}", parameters => DeleteResidentApartment(parameters.residentId, parameters.apartmentId));
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
            return HttpStatusCode.Conflict; // Slučaj ako je već dodijeljen

        var residentApartment = new ResidentApartment
        {
            ResidentId = residentId,
            ApartmentId = apartmentId
        };

        _dbContext.ResidentApartments.Add(residentApartment);
        _dbContext.SaveChanges();

        return HttpStatusCode.Created;
    }

    // PUT: Update the resident-apartment relationship
    private object UpdateResidentApartment(int residentId, int apartmentId)
    {
        var existingRelation = _dbContext.ResidentApartments
            .FirstOrDefault(ra => ra.ResidentId == residentId);

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