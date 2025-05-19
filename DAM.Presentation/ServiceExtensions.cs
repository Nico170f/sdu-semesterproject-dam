using DAM.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DAM.Presentation {
    public static class ServiceExtensions
    {

        public static IHostApplicationBuilder ConfigureAndAddDamServices(this IHostApplicationBuilder builder)
        {
            // Add services to the container.

            //builder.Services.AddScoped<ImageService>();

            builder.Services.AddScoped<CreateService>();
            builder.Services.AddScoped<ReadService>();
            builder.Services.AddScoped<UpdateService>();
            builder.Services.AddScoped<DeleteService>();
            
            builder.Services.AddHttpClient("DAMApi", client =>
            {
	            client.BaseAddress = new Uri("http://localhost:5115/");
            });

            
            return builder;            
        }
    }
}