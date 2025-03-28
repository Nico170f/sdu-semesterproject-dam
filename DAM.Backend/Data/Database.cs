using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DAM.Backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Data;

public sealed class Database : DbContext
{
    public DbSet<Image> Images { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TagImageGroup> TagImageGroups { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<ImageGroup> ImageGroups { get; set; }
    


    private static Database? _instance = null;

    public static Database Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new Database();
            }
            return _instance;
        }
    }

    public string DbPath { get; }

    private Database()
    {
        // Use platform-independent path handling
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        string dbFolderPath = Path.Combine(projectRoot, "Data", "Database");

        // Create the directory if it doesn't exist
        if (!Directory.Exists(dbFolderPath))
        {
            Directory.CreateDirectory(dbFolderPath);
        }

        DbPath = Path.Combine(dbFolderPath, "DAM_database.db");
        Console.WriteLine("Database path: " + DbPath);
        
        // Make sure the database is created
        this.Database.EnsureCreated();
    }

    // The following configures EF to create a Sqlite database file
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

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