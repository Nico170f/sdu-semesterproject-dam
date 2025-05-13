using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Product;

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
    private int _pageNumber = 1;
    private string _searchText = "";

    private List<Models.Asset> _productImages = [];
    private List<Models.Asset> _gallery = [];
        
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
	        _productImages.Add(new Models.Asset
	        {
		        UUID = assetId
	        });
        }

        List<Guid> galleryImageIds = await ReadService.GetAllAssetIds();
        foreach (Guid galleryImageId in galleryImageIds)
        {
	        _gallery.Add(new Models.Asset()
	        {
		        UUID = galleryImageId
	        });
        }
    }
    
    private async Task SearchButton()
    {
        
    }
    
    private async Task ProductImageRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 1
        var item = _productImages[indices.oldIndex];

        // add it to the new index in list 2
        _gallery.Insert(indices.newIndex, item);

        await DeleteService.RemoveAssetFromProduct(_productId, _productImages[indices.oldIndex].UUID);
        
        // remove the item from the old index in list 1
        _productImages.Remove(_productImages[indices.oldIndex]);
    }

    private async Task GalleryRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 2
        var item = _gallery[indices.oldIndex];

        // add it to the new index in list 1
        _productImages.Insert(indices.newIndex, item);
        
        await CreateService.AddAssetToProduct(_productId, item.UUID, indices.newIndex);
       // remove the item from the old index in list 2
        _gallery.Remove(_gallery[indices.oldIndex]);
    }
    
    private async Task ProductImageReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _productImages[indices.oldIndex];
    
        // Remove from old position
        _productImages.RemoveAt(indices.oldIndex);
    
        // Insert at new position
        _productImages.Insert(indices.newIndex, item);

        await UpdateService.UpdatePriority(_productId, item.UUID, indices.newIndex);
    }

    private void GalleryReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _gallery[indices.oldIndex];
    
        // Remove from old position
        _gallery.RemoveAt(indices.oldIndex);
        
        // Insert at new position
        _gallery.Insert(indices.newIndex, item);
    }
}
