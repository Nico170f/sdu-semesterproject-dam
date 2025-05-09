using DAM.Presentation.EnhancedModels;
using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Product;

public partial class Products : ComponentBase
{
	
	
	[Inject] private NavigationManager Navigation { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	
	private string searchProduct = ""; //variable that holds text in searchbar

    private bool _isLoaded = false;
    private string searchText = ""; // Holds the search text
    List<EnhancedProduct> products = [];

    private string _newProductName, _newProductUuid;

    private async Task AddNewProduct ()
    {
	    try
	    {
		    Models.Product product = new Models.Product()
		    {
			    Name = _newProductName,
			    UUID = new Guid(_newProductUuid)
		    };
		    await CreateService.UploadProductWithUUID(product);
	    }
	    catch (Exception e)
	    {
		    Console.WriteLine(e.Message);
	    }
    }

    protected override async Task OnInitializedAsync ()
    {
	    products = await ReadService.GetAllProducts() ?? [];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(100);
            _isLoaded = true;
            StateHasChanged();
        }
    }

    public void NavigateToHome ()
    {
        Navigation.NavigateTo("/dam");
    }

    public void OnImageClickNavigateTo(string productId)
    {
        Navigation.NavigateTo($"/dam/products/edit?productId={productId}");
    }
    
    private IEnumerable<EnhancedProduct> FilteredProducts()
    {
        if (string.IsNullOrWhiteSpace(searchProduct)) //if there is nothing in search product it searches for all the products
            return products;

        return products.Where(product =>
            product.Name.Contains(searchProduct, StringComparison.OrdinalIgnoreCase) || //returns based on product name and product id
            product.UUID.ToString().Contains(searchProduct, StringComparison.OrdinalIgnoreCase));
    }
    
    private void HandleSearch(ChangeEventArgs e)
    {
	    searchProduct = e.Value?.ToString() ?? string.Empty;
    }
}