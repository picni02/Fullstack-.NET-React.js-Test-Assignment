﻿using Nancy;
using ResidentManagementSystem.Services;
using ResidentManagementSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResidentManagementSystem.Modules
{
    public class StaticFileModule : NancyModule
    {
        private readonly GenerateDataService _generateDataService;
        private readonly DataTransferService _transferService;
        public StaticFileModule(GenerateDataService generateDataService, DataTransferService dataTransferService)
        {
            _generateDataService = generateDataService;
            _transferService = dataTransferService;

            Get("/", _ => 
            {
                var response = Response.AsFile("Frontend/index.html", "text/html");
                response.WithHeader("Cache-Control", "no-store, no-cache, must-revalidate");
                return response;
            });

            Get("/resident.html", _ =>
            {
                var response = Response.AsFile("Frontend/resident.html", "text/html");
                response.WithHeader("Cache-Control", "no-store, no-cache, must-revalidate");
                return response;
            });
            Get("/event.html", _ =>
            {
                var response = Response.AsFile("Frontend/event.html", "text/html");
                response.WithHeader("Cache-Control", "no-store, no-cache, must-revalidate");
                return response;
            });

            Get("static/{file}", args =>
            {
                var filePath = $"Frontend/static/{args.file}";
                if (File.Exists(filePath))
                {
                    return Response.AsFile(filePath);
                }
                else
                {
                    return HttpStatusCode.NotFound;
                }
            });

            Get("/transfer-data", _ =>
            {
                double timeUntilNextExecution = dataTransferService.GetTimeUntilNextMonday();
                DateTime nextTransfer = DateTime.Now.AddMilliseconds(timeUntilNextExecution);

                return Response.AsJson(nextTransfer);
            });

            Post("/transfer-data", async _ =>
            {
                try
                {
                   // var dataTransferService = new DataTransferService(new AppDbContext(), new ElasticSearchService());
                    await _transferService.TransferData();
                    return Response.AsJson(new { message = "Data transfer initiated successfully." });
                }
                catch(Exception ex)
                {
                    return Response.AsJson(new { error = ex.Message }, HttpStatusCode.InternalServerError);
                }
                
            });

            Post("/generate-data", async _ =>
            {
                try
                {
                    await _generateDataService.GenerateDataAsync();
                    return Response.AsJson(new { message = "Data generated successfully!" });
                }
                catch (Exception ex)
                {
                    return Response.AsJson(new { error = ex.Message }, HttpStatusCode.InternalServerError);
                }
            });
        }
    }
}
