using DAM.Backend.Services.ControllerServices;

namespace DAM.Backend;

public sealed class Webserver
{
    private static Webserver? _instance = null;

    private bool _isRunning = false;
    
    private Webserver()
    {}
    
    public static Webserver Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Webserver();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Starts the web server if it is not already running.
    /// </summary>
    /// <param name="args">Command line arguments to be passed to the web application.</param>
    /// <remarks>
    /// This method initializes and starts the web application by:
    /// - Creating a WebApplicationBuilder
    /// - Configuring services via AddServices
    /// - Building the WebApplication
    /// - Configuring the application pipeline via ConfigureApp
    /// - Running the web server
    /// 
    /// Once called, the server will remain running until the application is terminated.
    /// </remarks>
    public void Start(string[] args)
    {
        if(_isRunning) return;
        _isRunning = true;

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        AddServices(builder);

        WebApplication app = builder.Build();
        ConfigureApp(app);

        app.Run();
    }

    private void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            
        }
        
        app.UseHttpsRedirection();
        app.MapControllers();
    }

    private void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.AddDebug();
        });

        //RegisterDB(builder);

        builder.Services.AddScoped<IAssetService, AssetService>();
        RegisterWebControllers(builder);
    }


    private void RegisterWebControllers(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
    }

    // private void RegisterDB(WebApplicationBuilder builder)
    // {
    //     builder.Services.AddDbContext<ImageDbContext>(options => {
    //         options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    //     });
    // }
}
