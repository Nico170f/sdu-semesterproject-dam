namespace DAM.Presentation.Services;

public abstract class BaseService
{
	protected readonly HttpClient _httpClient;

	protected BaseService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("DAMApi");
	}
}