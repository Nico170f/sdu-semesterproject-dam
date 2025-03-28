using System.Net.Http;
using System.Net.Http.Json; // For easy JSON handling
using System.Threading.Tasks;

namespace DAM.Presentation;

public class DatabaseService
{
    private readonly HttpClient _httpClient;

    public DatabaseService(HttpClient httpClient) // Inject HttpClient
    {
        _httpClient = httpClient;
    }
}