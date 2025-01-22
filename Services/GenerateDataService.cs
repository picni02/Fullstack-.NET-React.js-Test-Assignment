using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using Bogus;
using System;
using System.Collections.Generic;

namespace ResidentManagementSystem.Services
{
    public class GenerateDataService
    {
        private readonly AppDbContext _dbContext;
        public GenerateDataService(AppDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task GenerateDataAsync()
        {
            const int batchSize = 5000;

            // Generisanje Residenata
            var residentFaker = new Faker<Resident>()
                .RuleFor(r => r.FirstName, f => f.Name.FirstName())
                .RuleFor(r => r.LastName, f => f.Name.LastName())
                .RuleFor(r => r.IsInside, f => f.Random.Bool());

            var residents = new List<Resident>();
            for (int i = 0; i < 100000; i += batchSize)
            {
                var batch = residentFaker.Generate(batchSize);
                residents.AddRange(batch);
                await _dbContext.Residents.AddRangeAsync(batch);
                await _dbContext.SaveChangesAsync(); // Batch SaveChanges
            }
            Console.WriteLine("Generated 100 000 residents.");

            // Generisanje Apartmana
            var apartmentFaker = new Faker<Apartment>()
                .RuleFor(a => a.ApartmentNumber, f => f.Random.Int(1, 100000).ToString())
                .RuleFor(a => a.Address, f => f.Address.FullAddress());

            //var apartments = apartmentFaker.Generate(50000);
            //await _dbContext.Apartments.AddRangeAsync(apartments);
            //await _dbContext.SaveChangesAsync();
            //Console.WriteLine("Generated 100,000 apartments.");

            var apartments = new List<Apartment>();
            for (int i = 0; i < 100000; i += batchSize)
            {
                var batch = apartmentFaker.Generate(batchSize);
                apartments.AddRange(batch);
                await _dbContext.Apartments.AddRangeAsync(batch);
                await _dbContext.SaveChangesAsync(); // Batch SaveChanges
            }
            Console.WriteLine("Generated 100 000 apartments.");

            // Generisanje ResidentApartment
            var residentApartmentFaker = new Faker<ResidentApartment>()
                .RuleFor(ra => ra.ResidentId, f => f.PickRandom(residents.Select(r => r.ResidentId).ToList()))
                .RuleFor(ra => ra.ApartmentId, f => f.PickRandom(apartments.Select(a => a.ApartmentId).ToList()));

            //var residentApartments = residentApartmentFaker.Generate(50000);
            //await _dbContext.ResidentApartments.AddRangeAsync(residentApartments);
            //await _dbContext.SaveChangesAsync();
            //Console.WriteLine("Generated 50,000 resident-apartment relationships.");

            var residentApartments = new List<ResidentApartment>();
            for (int i = 0; i < 100000; i += batchSize)
            {
                var batch = residentApartmentFaker.Generate(batchSize);
                residentApartments.AddRange(batch);
                await _dbContext.ResidentApartments.AddRangeAsync(batch);
                await _dbContext.SaveChangesAsync(); // Batch SaveChanges
            }
            Console.WriteLine("Generated 100 000 resident-apartments.");

            // Generisanje Evenata
            var eventFaker = new Faker<Event>()
                .RuleFor(e => e.EventTime, f => f.Date.Past(2, DateTime.Now))
                .RuleFor(e => e.EventType, f => f.PickRandom("ENTRY", "EXIT"))
                .RuleFor(e => e.ResidentId, f => f.PickRandom(residents.Select(r => r.ResidentId).ToList()))
                .RuleFor(e => e.ApartmentId, f => f.PickRandom(apartments.Select(a => a.ApartmentId).ToList()));

            for (int i = 0; i < 1000000; i += batchSize)
            {
                var batch = eventFaker.Generate(batchSize);
                await _dbContext.Events.AddRangeAsync(batch);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Generated {i + batchSize} events.");
            }

            Console.WriteLine("Data generation completed.");
        }

    }
}
