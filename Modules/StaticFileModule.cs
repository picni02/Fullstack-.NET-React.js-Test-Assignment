using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResidentManagementSystem.Modules
{
    public class StaticFileModule : NancyModule
    {
        public StaticFileModule()
        {
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
        }
    }
}
