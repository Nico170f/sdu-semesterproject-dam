
using DAM.Backend.Data;

namespace DAM.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        DummyDataGenerator.LoadData();
        Database database = Database.Instance;
        Webserver.Instance.Start(args);
    }
}