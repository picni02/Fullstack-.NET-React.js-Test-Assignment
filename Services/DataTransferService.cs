using ResidentManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Services
{
    public class DataTransferService
    {
        private readonly AppDbContext _appDbContext;
        public DataTransferService(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
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
