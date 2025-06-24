using DAM.Presentation.Services;
using DAM.Shared.Models;
using DAM.Shared.Responses;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Product_Pages;

public partial class Edit : ComponentBase
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
	
	private Guid _productId = Guid.Empty;
    private string _productName = "";
    private string _searchText = "";

    private List<Tag> _allTags = [];
    private HashSet<Tag> _selectedTags = [];

    private bool _showTagMenu = false;
    
    private List<Asset> _productAssets = [];
    private List<Asset> _assetGallery = [];

    private int _currentPageNumber = 1;
    private int _totalPageCount = 0;
    private int _amount = 18;
    protected override async Task OnInitializedAsync ()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("productId", out var id))
            _productId = new Guid(id);

        _productName = await ReadService.GetProductName(_productId);
        
        List<Guid> assetIds = await ReadService.GetAssetsByProduct(_productId); 
        foreach (Guid assetId in assetIds)
        {
	        _productAssets.Add(new Asset
	        {
		        UUID = assetId
	        });
        }

        _allTags = (await ReadService.GetTags())?.Tags ?? [];
        
        UpdateAssetGallery();
    }

    private void OnSearchInputChanged (ChangeEventArgs e)
    {
	    _searchText = e.Value?.ToString() ?? "";
	    UpdateAssetGallery();
    }
    
    private void ToggleTagMenu()
    {
	    _showTagMenu = !_showTagMenu;
    }
    
    private void OnTagFilterChanged(Tag tag, bool isChecked)
    {
	    if (isChecked)
	    {
		    _selectedTags.Add(tag);
	    }
	    else
	    {
		    _selectedTags.Remove(tag);
	    }
	    UpdateAssetGallery();
    }

    private async void UpdateAssetGallery()
    {
	    var response = await ReadService.GetAssets(null, _productId, _searchText, _selectedTags,amount: _amount, page: _currentPageNumber);
	    _totalPageCount = (int)Math.Ceiling(((response?.TotalCount??0) * 1.0f)/ _amount);
        _assetGallery = response?.Assets ?? [];
	    StateHasChanged();
    }
    
    private async Task ProductImageRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 1
        var item = _productAssets[indices.oldIndex];

        // add it to the new index in list 2
        _assetGallery.Insert(indices.newIndex, item);

        await DeleteService.RemoveAssetFromProduct(_productId, _productAssets[indices.oldIndex].UUID);
        
        // remove the item from the old index in list 1
        _productAssets.Remove(_productAssets[indices.oldIndex]);
    }

    private async Task GalleryRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 2
        var item = _assetGallery[indices.oldIndex];

        // add it to the new index in list 1
        _productAssets.Insert(indices.newIndex, item);
        
        await CreateService.AddAssetToProduct(_productId, item.UUID, indices.newIndex);
       // remove the item from the old index in list 2
       _assetGallery.Remove(_assetGallery[indices.oldIndex]);
    }
    
    private async Task ProductImageReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _productAssets[indices.oldIndex];
    
        // Remove from old position
        _productAssets.RemoveAt(indices.oldIndex);
    
        // Insert at new position
        _productAssets.Insert(indices.newIndex, item);

        await UpdateService.UpdatePriority(_productId, item.UUID, indices.newIndex);
    }

    private void GalleryReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _assetGallery[indices.oldIndex];
    
        // Remove from old position
        _assetGallery.RemoveAt(indices.oldIndex);
        
        // Insert at new position
        _assetGallery.Insert(indices.newIndex, item);
    }
    
    private void NextPage ()
    {
	    _currentPageNumber = int.Min(_totalPageCount, _currentPageNumber + 1);
	    UpdateAssetGallery();
    }

    private void PreviousPage ()
    {
	    _currentPageNumber = int.Max(1, _currentPageNumber - 1);
	    UpdateAssetGallery();
    }
}
