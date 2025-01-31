using Microsoft.EntityFrameworkCore;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using ResidentManagementSystem.Data;
using ResidentManagementSystem.Modules;
using ResidentManagementSystem.Services;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        var uri = new Uri("http://localhost:5000");
        var hostConfig = new HostConfiguration
        {
            UrlReservations = new UrlReservations { CreateAutomatically = true }
        };

        using (var host = new NancyHost(hostConfig, uri))
        {
            host.Start();
            Console.WriteLine("NancyFX host is running on http://localhost:5000");

            // Konfiguracija dependency injection container-a
            var container = new TinyIoCContainer();

            // Registracija servisa
            container.Register<AppDbContext>().AsSingleton();
            container.Register<ElasticSearchService>().AsSingleton();
            container.Register<DataTransferService>().AsSingleton();
            container.Register<GenerateDataService>().AsSingleton();
            container.Register<CorsModule>().AsSingleton();

            var elasticsearchService = container.Resolve<ElasticSearchService>();
            var dataTransferService = container.Resolve<DataTransferService>();
            var generateDataService = container.Resolve<GenerateDataService>();

            var client = elasticsearchService.GetClient();
            var response = client.Ping();

            if (response.IsValid)
            {
                Console.WriteLine("Elasticsearch is connected successfully!");
                elasticsearchService.EnsureIndexesExist();
            }
            else
            {
                Console.WriteLine($"Greška: {response.OriginalException.Message}");
            }

            // Pokretanje automatskog prenosa podataka svakog ponedjeljka u ponoć
            _ = StartDataTransferScheduler(dataTransferService);

            Console.ReadLine();
        }
    }

    //public static void ConfigureApplication(TinyIoCContainer container)
    //{
    //    container.Register(new AppDbContext());

    //}

    private static async Task StartDataTransferScheduler(DataTransferService dataTransferService)
    {
        while (true)
        {
            double timeUntilNextExecution = dataTransferService.GetTimeUntilNextMonday();
            Console.WriteLine($"Sljedeći prijenos podataka zakazan za: {DateTime.Now.AddMilliseconds(timeUntilNextExecution)}");

            await Task.Delay((int)timeUntilNextExecution); // Pauza do sljedećeg ponedjeljka u ponoć

            Console.WriteLine("Započinjem prijenos podataka u Elasticsearch...");
            await dataTransferService.TransferData();
            Console.WriteLine("Prijenos podataka završen.");
        }
    }
}
