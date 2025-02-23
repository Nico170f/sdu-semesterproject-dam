namespace App.Logic;

/// <summary>
/// A product class that holds information about a product such as name, ID and image URI.
/// Default image URI is set to a placeholder image of Henrik :D
/// </summary>
public class Product
{
	public string Name { get; set; }
	public string ID { get; set; }
	public string ImageURI { get; set; }
	
	public Product(string name, string id, string imageURI = "../Assets/Henrik.jpg")
	{
		Name = name;
		ID = id;
		ImageURI = imageURI;
	}
}