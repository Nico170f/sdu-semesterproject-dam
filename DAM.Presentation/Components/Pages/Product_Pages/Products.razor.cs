using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Product_Pages;

public partial class Products : ComponentBase
{
	
	#region Injects
	#pragma warning disable CS8618
	[Inject] private NavigationManager NavigationManager { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	#pragma warning disable CS8618
	#endregion
	
    private string _searchText = "";
    private List<Shared.Models.Product> _products = [];
    
    private int _amount = 20;

    private int _currentPageNumber = 1;
    private int _totalPageCount = 0;
    
    protected override void OnInitialized ()
    {
	    UpdateProductList();
    }
    
    private void OnSearchInputChanged (ChangeEventArgs e)
    {
	    _searchText = e.Value?.ToString() ?? "";
	    UpdateProductList();
    }
    
    private async void UpdateProductList ()
    {
	    var response = await ReadService.GetProducts(searchString: _searchText, amount: _amount, page: _currentPageNumber);

	    if(response is null) return;

	    _products = response.Products;
	    
	    if (response.TotalCount.HasValue)
	    {
		    int totalAmount = response.TotalCount.Value;
		    _totalPageCount = (int)Math.Ceiling(totalAmount * 1.0f / _amount);
	    }
	    
	    StateHasChanged();
    }

    private async Task SyncWithPim ()
    {
	    await ReadService.SyncWithPim();
	    UpdateProductList();
    }
    
    private void NextPage ()
    {
	    _currentPageNumber = int.Min(_totalPageCount, _currentPageNumber + 1);
	    UpdateProductList();
    }

    private void PreviousPage ()
    {
	    _currentPageNumber = int.Max(1, _currentPageNumber - 1);
		UpdateProductList();
    }
}