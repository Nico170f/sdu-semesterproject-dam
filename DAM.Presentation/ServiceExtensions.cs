using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DAM.Presentation {
    public static class ServiceExtensions
    {

        public static IHostApplicationBuilder ConfigureAndAddDamServices(this IHostApplicationBuilder builder)
        {
            // Add services to the container.

            //builder.Services.AddScoped<ImageService>();
            
            return builder;            
        }
    }
}