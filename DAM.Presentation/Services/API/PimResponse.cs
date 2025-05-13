namespace DAM.Presentation.Services.API;

public class PimResponse
{
	public List<Item> items { get; set; }
	public int page { get; set; }
	public int number_of_pages { get; set; }
}

public class Item
{
	public string product_id { get; set; }
	public string name { get; set; }
}