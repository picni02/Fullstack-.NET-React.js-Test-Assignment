using ResidentManagementSystem.Data;
using ResidentManagementSystem.Models;
using Bogus;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.Extensions.Logging;

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
            const int batchSize = 1000;
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            var residentFaker = new Faker<Resident>()
                .RuleFor(r => r.FirstName, f => f.Name.FirstName())
                .RuleFor(r => r.LastName, f => f.Name.LastName())
                .RuleFor(r => r.IsInside, f => f.Random.Bool());

            var residents = new List<Resident>();
            var generatedResidents = residentFaker.Generate(1000);
            residents.AddRange(generatedResidents);
            for(int i = 0; i < residents.Count; i+= batchSize)
            {
                var batch = residents.Skip(i).Take(batchSize).ToList();
                await InsertResidentsBatchAsync(batch);
            }
            
            Console.WriteLine("Generated 100 000 residents.");

            var apartmentFaker = new Faker<Apartment>()
                .RuleFor(a => a.ApartmentNumber, f => f.Random.Int(1, 150000).ToString())
                .RuleFor(a => a.Address, f => f.Address.FullAddress());

            var apartments = new List<Apartment>();
            var genratedApartments = apartmentFaker.Generate(1000);
            apartments.AddRange(genratedApartments);
            for(int i = 0; i< apartments.Count; i+= batchSize)
            {
                var batch = apartments.Skip(i).Take(batchSize).ToList();
                await InsertApartmentsBatchAsync(batch);
            }
            
            Console.WriteLine("Generated 100 000 apartments.");

            var residentIds = _dbContext.Residents.Select(r => r.ResidentId).ToList();
            var apartmentIds = _dbContext.Apartments.Select(a => a.ApartmentId).ToList();
   
            var residentApartmentFaker = new Faker<ResidentApartment>()
                .RuleFor(ra => ra.ResidentId, f => f.PickRandom(residentIds))
                .RuleFor(ra => ra.ApartmentId, f => f.PickRandom(apartmentIds));

            var residentApartments = new List<ResidentApartment>();
            var generatedResidentApartments = residentApartmentFaker.Generate(500);
            residentApartments.AddRange(generatedResidentApartments);
            for(int i = 0; i < residentApartments.Count; i+= 100)
            {
                var batch = residentApartments.Skip(i).Take(100).ToList();
                await InsertResidentApartmentsBatchAsync(batch);
            }
            
            Console.WriteLine("Generated 50 000 resident-apartments.");

            var events = new List<Event>();
            var eventFaker = new Faker<Event>()
                .RuleFor(e => e.EventTime, f => f.Date.Past(2, DateTime.Now))
                .RuleFor(e => e.EventType, f => f.PickRandom("ENTRY", "EXIT"))
                .RuleFor(e => e.ResidentId, f => f.PickRandom(residentIds))
                .RuleFor(e => e.ApartmentId, f => f.PickRandom(apartmentIds));

            var generatedEvents = eventFaker.Generate(5000);
            events.AddRange(generatedEvents);
            for(int i = 0; i < events.Count;  i+= batchSize)
            {
                var batch = events.Skip(i).Take(batchSize).ToList();
                await InsertEventsBatchAsync(batch);
            }
            
            Console.WriteLine("Generated 1 000 000 events.");

            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            Console.WriteLine("Data generation completed.");
        }

        private async Task InsertResidentsBatchAsync(List<Resident> residents)
        {
            if (residents == null || residents.Count == 0)
            {
                return;
            }
            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder("INSERT INTO residents (FirstName, LastName, IsInside) VALUES ");
                var parameters = new List<string>();

                for (int i = 0; i < residents.Count; i++)
                {
                    parameters.Add($"(@FirstName{i}, @LastName{i}, @IsInside{i})");
                    command.Parameters.Add(new MySqlParameter($"@FirstName{i}", residents[i].FirstName));
                    command.Parameters.Add(new MySqlParameter($"@LastName{i}", residents[i].LastName));
                    command.Parameters.Add(new MySqlParameter($"@IsInside{i}", residents[i].IsInside));
                }

                sql.Append(string.Join(",", parameters));
                command.CommandText = sql.ToString();
                await command.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }

        private async Task InsertApartmentsBatchAsync(List<Apartment> apartments)
        {
            if (apartments == null || apartments.Count == 0)
            {
                return;
            }

            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder("INSERT INTO apartments (ApartmentNumber, Address) VALUES ");
                var parameters = new List<string>();

                for (int i = 0; i < apartments.Count; i++)
                {
                    parameters.Add($"(@ApartmentNumber{i}, @Address{i})");
                    command.Parameters.Add(new MySqlParameter($"@ApartmentNumber{i}", apartments[i].ApartmentNumber));
                    command.Parameters.Add(new MySqlParameter($"@Address{i}", apartments[i].Address));
                }

                sql.Append(string.Join(",", parameters));
                command.CommandText = sql.ToString();
                await command.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }


        private async Task InsertResidentApartmentsBatchAsync(List<ResidentApartment> residentApartments)
        {
            if (residentApartments == null || residentApartments.Count == 0)
            {
                return;
            }

            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder("INSERT IGNORE INTO residentapartments (ResidentId, ApartmentId) VALUES ");
                var parameters = new List<string>();

                for (int i = 0; i < residentApartments.Count; i++)
                {
                    parameters.Add($"(@ResidentId{i}, @ApartmentId{i})");
                    command.Parameters.Add(new MySqlParameter($"@ResidentId{i}", residentApartments[i].ResidentId));
                    command.Parameters.Add(new MySqlParameter($"@ApartmentId{i}", residentApartments[i].ApartmentId));
                }

                sql.Append(string.Join(",", parameters));
                command.CommandText = sql.ToString();
                await command.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }



        private async Task InsertEventsBatchAsync(List<Event> events)
        {
            if (events == null || events.Count == 0)
            {
                return;
            }
            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder("INSERT IGNORE INTO events (EventTime, ResidentId, EventType, ApartmentId) VALUES ");
                var parameters = new List<string>();

                for (int i = 0; i < events.Count; i++)
                {
                    parameters.Add($"(@EventTime{i}, @ResidentId{i}, @EventType{i}, @ApartmentId{i})");
                    command.Parameters.Add(new MySqlParameter($"@EventTime{i}", events[i].EventTime));
                    command.Parameters.Add(new MySqlParameter($"@ResidentId{i}", events[i].ResidentId));
                    command.Parameters.Add(new MySqlParameter($"@EventType{i}", events[i].EventType));
                    command.Parameters.Add(new MySqlParameter($"@ApartmentId{i}", events[i].ApartmentId));
                }

                sql.Append(string.Join(",", parameters));
                command.CommandText = sql.ToString();
                await command.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }
    }
}
