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
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")) // URL for Elasticsearch instance
                .DefaultIndex("residents"); // Default index is residents

            _client = new ElasticClient(settings);

            // Connection check
            var pingResponse = _client.Ping();
            if (!pingResponse.IsValid)
            {
                Console.WriteLine($"Elasticsearch is not available: {pingResponse.DebugInformation}");
                throw new Exception("Error connecting with Elasticsearch");
            }
        }

        public void EnsureIndexesExist()
        {
            EnsureResidentsIndex();
            EnsureEventsIndex();
            EnsureApartmentsIndex();
            EnsureResidentApartmentsIndex();
        }

        private void EnsureResidentsIndex()
        {
            var existsResponse = _client.Indices.Exists("residents");
            if (!existsResponse.Exists)
            {
                var createIndexResponse = _client.Indices.Create("residents", c => c
                    .Map(m => m
                        .Properties(p => p
                            .Number(n => n.Name("residentId"))
                            .Text(t => t.Name("firstName"))
                            .Text(t => t.Name("lastName"))
                            .Boolean(b => b.Name("isInside"))
                        )
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine("Error creating residents index: " + createIndexResponse.DebugInformation);
                }
            }
            else
            {
                Console.WriteLine("Index residents already exists.");
            }
        }

        private void EnsureEventsIndex()
        {
            var existsResponse = _client.Indices.Exists("events");
            if (!existsResponse.Exists)
            {
                var createIndexResponse = _client.Indices.Create("events", c => c
                    .Map(m => m
                        .Properties(p => p
                            .Number(n => n.Name("eventId"))
                            .Date(d => d.Name("eventTime"))
                            .Number(n => n.Name("residentId"))
                            .Keyword(k => k.Name("eventType"))
                            .Number(n => n.Name("apartmentId"))
                        )
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine("Error creating events index: " + createIndexResponse.DebugInformation);
                }
            }
            else
            {
                Console.WriteLine("Index events already exists.");
            }
        }

        private void EnsureApartmentsIndex()
        {
            var existsResponse = _client.Indices.Exists("apartments");
            if (!existsResponse.Exists)
            {
                var createIndexResponse = _client.Indices.Create("apartments", c => c
                    .Map(m => m
                        .Properties(p => p
                            .Number(n => n.Name("apartmentId"))
                            .Keyword(k => k.Name("apartmentNumber"))
                            .Text(t => t.Name("address"))
                        )
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine("Error creating apartments index: " + createIndexResponse.DebugInformation);
                }
            }
            else
            {
                Console.WriteLine("Index apartments already exists.");
            }
        }

        private void EnsureResidentApartmentsIndex()
        {
            var existsResponse = _client.Indices.Exists("residentapartments");
            if (!existsResponse.Exists)
            {
                var createIndexResponse = _client.Indices.Create("residentapartments", c => c
                    .Map(m => m
                        .Properties(p => p
                            .Number(n => n.Name("residentId"))
                            .Number(n => n.Name("apartmentId"))
                        )
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine("Error creating residentapartments index: " + createIndexResponse.DebugInformation);
                }
            }
            else
            {
                Console.WriteLine("Index residentapartments already exists.");
            }
        }

        public ElasticClient GetClient()
        {
            return _client;
        }

        // Resident CRUD operations
        public static void IndexResident(Resident resident)
        {
            var indexResident = _client.Index(resident, idx => idx.Index("residents"));
            if (!indexResident.IsValid)
            {
                Console.WriteLine($"Failed to index resident: {indexResident.DebugInformation}");
            }
        }

        public static async Task BulkIndexResidentsAsync(IEnumerable<Resident> residents)
        {
            var batchSize = 10000;
            var residentList = residents.ToList();
            var totalBatches = (int)Math.Ceiling((double)residentList.Count / batchSize);
            
            for(int i=0; i < totalBatches; i++)
            {
                var batch = residentList.Skip(i* batchSize).Take(batchSize);

                var bulkRequest = new BulkDescriptor();

                foreach (var resident in batch)
                {
                    bulkRequest.Index<Resident>(op => op
                        .Index("residents")
                        .Document(resident));
                }

                var bulkResponse = await _client.BulkAsync(bulkRequest);

                if (bulkResponse.Errors)
                {
                    foreach (var itemWithError in bulkResponse.ItemsWithErrors)
                    {
                        Console.WriteLine($"Failed to index resident with ID {itemWithError.Id}: {itemWithError.Error.Reason}");
                    }
                }
                else
                {
                    Console.WriteLine($"Successfully indexed {i + 1}/{totalBatches} residents.");
                }
            }

            
        }


        public static Resident GetResidentById(int id)
        {
            var searchResponse = _client.Get<Resident>(id, idx => idx.Index("residents"));
            return searchResponse.IsValid ? searchResponse.Source : null;
        }

        public static void DeleteResidentById(int id)
        {
            var deleteResponse = _client.Delete<Resident>(id, idx => idx.Index("residents"));
            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete resident: {deleteResponse.DebugInformation}");
            }
        }

        // Event CRUD operations
        public static void IndexEvent(Event residentEvent)
        {
            var indexResponse = _client.Index(residentEvent, idx => idx.Index("events"));
            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Failed to index event: {indexResponse.DebugInformation}");
            }
        }

        public static async Task BulkIndexEventsAsync(IEnumerable<Event> events)
        {
            var bulkRequest = new BulkDescriptor();

            foreach (var ev in events)
            {
                bulkRequest.Index<Event>(op => op
                    .Index("events")
                    .Document(ev));
            }

            var bulkResponse = await _client.BulkAsync(bulkRequest);

            if (bulkResponse.Errors)
            {
                foreach (var itemWithError in bulkResponse.ItemsWithErrors)
                {
                    Console.WriteLine($"Failed to index event with ID {itemWithError.Id}: {itemWithError.Error.Reason}");
                }
            }
            else
            {
                Console.WriteLine($"Successfully indexed {events.Count()} events.");
            }
        }

        public static List<Event> GetEventsByResidentId(int residentId)
        {
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
            var deleteResponse = _client.Delete<Event>(eventId, idx => idx.Index("events"));
            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete event: {deleteResponse.DebugInformation}");
            }
        }

        // Apartment CRUD operations
        public static void IndexApartment(Apartment apartment)
        {
            var indexResponse = _client.Index(apartment, idx => idx.Index("apartments"));
            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Failed to index apartment: {indexResponse.DebugInformation}");
            }
        }

        public static async Task BulkIndexApartmentsAsync(IEnumerable<Apartment> apartments)
        {
            var bulkRequest = new BulkDescriptor();

            foreach (var apartment in apartments)
            {
                bulkRequest.Index<Apartment>(op => op
                    .Index("apartments")
                    .Document(apartment));
            }

            var bulkResponse = await _client.BulkAsync(bulkRequest);

            if (bulkResponse.Errors)
            {
                foreach (var itemWithError in bulkResponse.ItemsWithErrors)
                {
                    Console.WriteLine($"Failed to index apartment with ID {itemWithError.Id}: {itemWithError.Error.Reason}");
                }
            }
            else
            {
                Console.WriteLine($"Successfully indexed {apartments.Count()} apartments.");
            }
        }

        public static Apartment GetApartmentById(int id)
        {
            var searchResponse = _client.Get<Apartment>(id, idx => idx.Index("apartments"));
            return searchResponse.IsValid ? searchResponse.Source : null;
        }

        public static void DeleteApartmentById(int id)
        {
            var deleteResponse = _client.Delete<Apartment>(id, idx => idx.Index("apartments"));
            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete apartment: {deleteResponse.DebugInformation}");
            }
        }

        // ResidentApartment CRUD operations
        public static void IndexResidentApartment(ResidentApartment residentApartment)
        {
            var indexResponse = _client.Index(residentApartment, idx => idx.Index("residentapartments"));
            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Failed to index resident-apartment relationship: {indexResponse.DebugInformation}");
            }
        }

        public static async Task BulkIndexResidentApartmentsAsync(IEnumerable<ResidentApartment> residentApartments)
        {
            var bulkRequest = new BulkDescriptor();

            foreach (var residentapartment in residentApartments)
            {
                bulkRequest.Index<ResidentApartment>(ra => ra
                    .Index("residentapartments")
                    .Document(residentapartment));
            }

            var bulkResponse = await _client.BulkAsync(bulkRequest);

            if (bulkResponse.Errors)
            {
                foreach (var itemWithError in bulkResponse.ItemsWithErrors)
                {
                    Console.WriteLine($"Failed to index resident-apartment with ID {itemWithError.Id}: {itemWithError.Error.Reason}");
                }
            }
            else
            {
                Console.WriteLine($"Successfully indexed {residentApartments.Count()} resident-apartments.");
            }
        }

        public static List<ResidentApartment> GetResidentApartmentsByResidentId(int residentId)
        {
            var searchResponse = _client.Search<ResidentApartment>(s => s
                .Index("residentapartments")
                .Query(q => q
                    .Term(t => t.Field(f => f.ResidentId).Value(residentId))
                )
            );

            return searchResponse.IsValid ? searchResponse.Documents.ToList() : new List<ResidentApartment>();
        }

        public static void DeleteResidentApartmentById(int residentId, int apartmentId)
        {
            var deleteResponse = _client.DeleteByQuery<ResidentApartment>(q => q
                .Index("residentapartments")
                .Query(rq => rq
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t.ResidentId, residentId),
                            m => m.Term(t => t.ApartmentId, apartmentId)
                        )
                    )
                )
            );

            if (!deleteResponse.IsValid)
            {
                Console.WriteLine($"Failed to delete resident-apartment relationship: {deleteResponse.DebugInformation}");
            }
        }
    }
}
