using DAM.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Data;

public sealed class Database : DbContext
{
    public DbSet<Asset> Asset { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductAsset> ProductAssets { get; set; }	
	public DbSet<AssetTags> AssetTags { get; set; }
	
	public Database(DbContextOptions<Database> options) : base(options)
	{
		Database.EnsureCreated();
	}

    public static string GetDatabasePath()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        string dbFolderPath = Path.Combine(projectRoot, "Data", "Database");

        if (!Directory.Exists(dbFolderPath))
        {
            Directory.CreateDirectory(dbFolderPath);
        }

        return Path.Combine(dbFolderPath, "DAM_database.db");
    }
	
	
    // The following configures EF to create a Sqlite database file
    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    //     => options.UseSqlite($"Data Source={DbPath}");

	protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite($"Data Source=" + GetDatabasePath());
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    
        // Configure Asset model
        modelBuilder.Entity<Asset>()
            .Property(i => i.UUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        // Configure Tag model
        modelBuilder.Entity<Tag>()
            .Property(i => i.UUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        // Configure Product model
        modelBuilder.Entity<Product>()
            .Property(i => i.UUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        // Configure AssetTags model
        modelBuilder.Entity<AssetTags>()
            .Property(i => i.AssetUUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        modelBuilder.Entity<AssetTags>()
            .Property(i => i.TagUUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        modelBuilder.Entity<AssetTags>()
            .HasKey(it => new { it.AssetUUID, it.TagUUID });

        modelBuilder.Entity<AssetTags>()
            .HasOne<Asset>()
            .WithMany()
            .HasForeignKey(it => it.AssetUUID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetTags>()
            .HasOne<Tag>()
            .WithMany()
            .HasForeignKey(it => it.TagUUID)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure AssetTags ProductAssets
        modelBuilder.Entity<ProductAsset>()
            .Property(i => i.ProductUUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        modelBuilder.Entity<ProductAsset>()
            .Property(i => i.AssetUUID)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v)
            );
        
        modelBuilder.Entity<ProductAsset>()
            .HasKey(pi => new { pi.ProductUUID, pi.AssetUUID });
        
        modelBuilder.Entity<ProductAsset>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(pi => pi.ProductUUID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductAsset>()
            .HasOne<Asset>()
            .WithMany()
            .HasForeignKey(pi => pi.AssetUUID)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        base.OnModelCreating(modelBuilder);
    }
	
    // Create
    public async Task<bool> Create<T>(T entity) where T : class
    {
        try
        {
            Set<T>().Add(entity);
            await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating entity: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return false;
        }
    }

    // Read (Get one by ID)
    public async Task<T?> Read<T>(object id) where T : class
    {
        try
        {
            return await Set<T>().FindAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading entity: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return null;
        }
    }

    // Read (Get all)
    public async Task<List<T>> ReadAll<T>() where T : class
    {
        try
        {
            return await Set<T>().ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading entities: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return new List<T>();
        }
    }

    // Update
    public async Task<bool> Update<T>(T entity) where T : class
    {
        try
        {
            Set<T>().Update(entity);
            await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating entity: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return false;
        }
    }

    // Delete
    public async Task<bool> Delete<T>(T entity) where T : class
    {
        try
        {
            Set<T>().Remove(entity);
            await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting entity: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return false;
        }
    }

    // Delete by ID
    public async Task<bool> Delete<T>(object id) where T : class
    {
        try
        {
            var entity = await Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            Set<T>().Remove(entity);
            await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting entity: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            return false;
        }
    }
}