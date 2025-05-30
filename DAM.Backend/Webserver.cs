using DAM.Backend.Services.ControllerServices;
using DAM.Backend.Services.Formatters;
using DAM.Backend.Data;
using DAM.Backend.Services;

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
        {} 

        app.UseExceptionHandler("/error");
        app.UseCors(options =>
        {
            ///////options.AllowCredentials();
            /////////options.WithOrigins(Config.Database.origin);
            /////////options.WithMethods("GET", "POST", "DELETE");
            options.AllowAnyHeader();
        });
        
        app.UseHttpsRedirection();
        app.MapControllers();
    }

    private void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.OutputFormatters.Add(new FileOutputFormatter());
        });
        
        builder.Services.AddControllers().AddNewtonsoftJson();
        
        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.AddDebug();
        });

        //RegisterDB(builder);

        // todo: do we need to access the context? 
        // we probable need this for the file output formatter
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IAssetService, AssetService>();
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        // builder.Services.AddSingleton<AssetService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5000") // Your frontend URL
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
		
		// builder.Services.AddTransient<DbContext, Database>();
        
        builder.Services.AddDbContext<Database>();

        var config = builder.Configuration.GetSection("DefaultImages");
        var notFoundImage = config["NotFound"];
        if (notFoundImage != null)
        {
            HelperService.DefaultImage = notFoundImage;
        }
        else
        {
            throw new Exception("Default image not found in configuration.");
        }
            
    }
}
