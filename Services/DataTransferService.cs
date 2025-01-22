using Microsoft.EntityFrameworkCore;
using Nest;
using ResidentManagementSystem.Data;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using ResidentManagementSystem.Models;

namespace ResidentManagementSystem.Services
{
    public class DataTransferService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ElasticClient _elasticClient;
        private readonly System.Timers.Timer _timer;
        public DataTransferService(AppDbContext appDbContext, ElasticSearchService elasticSearchService) 
        {
            _appDbContext = appDbContext;
            _elasticClient = elasticSearchService.GetClient();

            _timer = new System.Timers.Timer(GetTimeUntilNextMonday());
            _timer.Elapsed += async (sender, e) => await TransferData();
            _timer.AutoReset = true;
            _timer.Start();
            
        }

        public double GetTimeUntilNextMonday()
        {
            DateTime now = DateTime.Now;
            DateTime nextMonday = now.AddDays((int)(DayOfWeek.Monday - now.DayOfWeek + 7) % 7).Date;
            DateTime executionTime = nextMonday.AddHours(0).AddMinutes(0).AddSeconds(0);

            if(executionTime < now)
            {
                executionTime = executionTime.AddDays(7);
            }

            return (executionTime - now).TotalMilliseconds;
        }

        public async Task TransferData()
        {
            Console.WriteLine("Starting data transfer to Elasticsearch...");
            var batchSize = 500;

            var totalResidents = _appDbContext.Residents.Count();
            for(int i = 0; i < totalResidents; i+=batchSize)
            {
                var residentsBatch = _appDbContext.Residents
                    .AsNoTracking()
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                await ElasticSearchService.BulkIndexResidentsAsync(residentsBatch);
            }
            
            Console.WriteLine("Residents data transferred.");


            var totalApartments = _appDbContext.Apartments.Count();
            for (int i = 0; i < totalApartments; i += batchSize)
            {
                var apartmentsBatch = _appDbContext.Apartments
                    .AsNoTracking()
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                await ElasticSearchService.BulkIndexApartmentsAsync(apartmentsBatch);
            }

            Console.WriteLine("Apartments data transferred.");


            var totalResidentApartments = _appDbContext.ResidentApartments.Count();
            for (int i = 0; i < totalResidentApartments; i += batchSize)
            {
                var residentApartmentsBatch = _appDbContext.ResidentApartments
                    .AsNoTracking()
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                await ElasticSearchService.BulkIndexResidentApartmentsAsync(residentApartmentsBatch);
            }

            Console.WriteLine("Resident-Apartments data transferred.");


            var totalEvents = _appDbContext.Events.Count();
            for (int i = 0; i < totalEvents; i += batchSize)
            {
                var eventsBatch = _appDbContext.Events
                    .AsNoTracking()
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                await ElasticSearchService.BulkIndexEventsAsync(eventsBatch);
            }

            Console.WriteLine("Events data transferred.");

         //   _appDbContext.Residents.RemoveRange(residents);
         //   _appDbContext.Apartments.RemoveRange(apartments);

         //   await _appDbContext.SaveChangesAsync();

            for (int i = 0; i < totalResidents; i += batchSize)
            {
                var batchToRemove = _appDbContext.Residents.AsNoTracking().Take(batchSize).ToList();
                _appDbContext.Residents.RemoveRange(batchToRemove);
                await _appDbContext.SaveChangesAsync();
            }

            for (int i = 0; i < totalApartments; i += batchSize)
            {
                var batchToRemove = _appDbContext.Apartments.AsNoTracking().Take(batchSize).ToList();
                _appDbContext.Apartments.RemoveRange(batchToRemove);
                await _appDbContext.SaveChangesAsync();
            }

            Console.WriteLine("Data transfer completed successfully.");
        }

        public async Task TransferResidentsAndEventsToElasticSearch()
        {
            var residents = _appDbContext.Residents.ToList();
            foreach (var res in residents)
            {
                ElasticSearchService.IndexResident(res);
            }

            var events = _appDbContext.Events.ToList();
            foreach(var e in events)
            {
                ElasticSearchService.IndexEvent(e);
            }

            _appDbContext.Residents.RemoveRange(residents);
            _appDbContext.Events.RemoveRange(events);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
