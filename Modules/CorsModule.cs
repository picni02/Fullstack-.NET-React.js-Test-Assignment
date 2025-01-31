using Nancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResidentManagementSystem.Modules
{
    public class CorsModule : NancyModule
    {
        public CorsModule()
        {
            Before += ctx =>
            {
                // Dopuštanje svih domena za CORS
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*");
                ctx.Response.WithHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                ctx.Response.WithHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // Ako je OPTIONS metoda, odmah odgovaramo sa 200
                if (ctx.Request.Method == "OPTIONS")
                {
                    return Response.AsJson(new { });
                }

                return null; // Pustite ostatak obrade zahtjeva da ide
            };
        }
    }
}
