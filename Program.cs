using Microsoft.EntityFrameworkCore;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using ResidentManagementSystem.Data;
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
            Console.ReadLine();
        }

        var elasticsearchService = new ElasticSearchService();
        var client = elasticsearchService.GetClient();

        var response = client.Ping();

        if (response.IsValid)
        {
            Console.WriteLine("Elasticsearch je uspješno povezan!");
        }
        else
        {
            Console.WriteLine($"Greška: {response.OriginalException.Message}");
        }

       // var dataTransferService = new DataTransferService(new AppDbContext());
       // await dataTransferService.TransferResidentsAndEventsToElasticSearch();


    }

    public static void ConfigureApplication(TinyIoCContainer container)
    {
        container.Register(new AppDbContext());

    }
}
