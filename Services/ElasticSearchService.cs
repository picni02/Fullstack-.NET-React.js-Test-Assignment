using Nest;
using ResidentManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Services
{
    public class ElasticSearchService
    {
        private static ElasticClient _client;

        public ElasticSearchService()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")) // URL for your Elasticsearch instance
                .DefaultIndex("residents"); // Default index is residents, but you can change it dynamically

            _client = new ElasticClient(settings);

            // Provjera konekcije
            var pingResponse = _client.Ping();
            if (!pingResponse.IsValid)
            {
                Console.WriteLine($"Elasticsearch nije dostupan: {pingResponse.DebugInformation}");
                throw new Exception("Greška pri spajanju na Elasticsearch");
            }
        }

        public ElasticClient GetClient()
        {
            return _client;
        }

        // Resident CRUD operations
        public static void IndexResident(Resident resident)
        {
            // Postavite indeks specifičan za residente
            var indexResident = _client.Index(resident, idx => idx.Index("residents"));
            if (!indexResident.IsValid)
            {
                Console.WriteLine($"Failed to index resident: {indexResident.DebugInformation}");
            }
        }

        public static Resident GetResidentById(int id)
        {
            // Postavite indeks specifičan za residente
            var searchResponse = _client.Get<Resident>(id, idx => idx.Index("residents"));
            return searchResponse.IsValid ? searchResponse.Source : null;
        }

        public static void DeleteResidentById(int id)
        {
            // Postavite indeks specifičan za residente
            var deleteResponse = _client.Delete<Resident>(id, idx => idx.Index("residents"));
            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete resident: {deleteResponse.DebugInformation}");
            }
        }

        // Event CRUD operations
        public static void IndexEvent(Event residentEvent)
        {
            // Postavite indeks specifičan za evente
            var indexResponse = _client.Index(residentEvent, idx => idx.Index("events"));
            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Failed to index event: {indexResponse.DebugInformation}");
            }
        }

        public static List<Event> GetEventsByResidentId(int residentId)
        {
            // Postavite indeks specifičan za evente
            var searchResponse = _client.Search<Event>(s => s
                .Index("events")
                .Query(q => q
                    .Term(t => t.Field(f => f.ResidentId).Value(residentId))
                )
            );

            return searchResponse.IsValid ? searchResponse.Documents.ToList() : new List<Event>();
        }

        public static void DeleteEventById(int eventId)
        {
            // Postavite indeks specifičan za evente
            var deleteResponse = _client.Delete<Event>(eventId, idx => idx.Index("events"));
            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete event: {deleteResponse.DebugInformation}");
            }
        }
    }
}
