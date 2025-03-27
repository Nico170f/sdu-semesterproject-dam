using DAM.Backend.Data;

namespace DAM.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        // Initialize database
        Database database = Database.Instance;
        
        // Check if products already exist before creating them
        if (database.Product.Count() == 0)
        {
            // Only create products if none exist
            DummyDataGenerator.CreateProducts();
        }
        
        Webserver.Instance.Start(args);
    }
}