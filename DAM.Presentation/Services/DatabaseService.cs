// For easy JSON handling

namespace DAM.Presentation.Services;

public class DatabaseService
{
    private readonly HttpClient _httpClient;

    public DatabaseService(HttpClient httpClient) // Inject HttpClient
    {
        _httpClient = httpClient;
    }
}