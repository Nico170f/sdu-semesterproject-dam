using DAM.Presentation.EnhancedModels;
using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Product;

public partial class Products : ComponentBase
{
	
	
	#region Injects
	#pragma warning disable CS8618
	[Inject] private NavigationManager Navigation { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	#pragma warning disable CS8618
	#endregion
	
    private bool _isLoaded = false;
    private string _searchText = "";
    private List<EnhancedProduct> _products = [];

    private string _newProductName, _newProductUuid;



    protected override async Task OnInitializedAsync ()
    {
	    _products = await ReadService.GetAllProducts() ?? [];
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
    
    private void OnSearchInputChanged (ChangeEventArgs e)
    {
	    _searchText = e.Value?.ToString() ?? "";
	    UpdateProductList();
    }
    
    private async void UpdateProductList ()
    {
	    _products = await ReadService.GetProducts(searchString: _searchText);
	    StateHasChanged();
    }

    private async Task SyncWithPim ()
    {
	    await ReadService.SyncWithPim();
	    UpdateProductList();
    }
    
    //not used
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
}