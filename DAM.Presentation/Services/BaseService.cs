namespace DAM.Presentation.Services;

public abstract class BaseService(IHttpClientFactory httpClientFactory)
{
	protected readonly HttpClient HttpClient = httpClientFactory.CreateClient("DAMApi");
}