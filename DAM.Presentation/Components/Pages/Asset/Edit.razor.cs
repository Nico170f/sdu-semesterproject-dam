using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Asset;

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
	
	private Guid _assetId = Guid.Empty;
    private string _searchText = "";
    
    private List<Models.Tag> _imageTags = [];
    private List<Models.Tag> _list = [];
        
    protected override async Task OnInitializedAsync ()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("assetId", out var id))
	        _assetId = new Guid(id);

        _imageTags = await ReadService.GetTagsByAsset(_assetId);

        _list = await ReadService.GetTagsNotOnAsset(_assetId);

    }
    
    private async Task SearchButton()
    {
	    /*
	    foreach (Models.Tag tag in _list)
	    {
		    tag.IsShown = true;
		    if (!tag.Name.Contains(_searchText))
		    {
			    tag.IsShown = false;
		    }
	    }
	    */
    }
    
    private async Task ImageTagsRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 1
        var item = _imageTags[indices.oldIndex];

        // add it to the new index in list 2
        _list.Insert(indices.newIndex, item);

        await DeleteService.RemoveTagFromAsset(_assetId, _imageTags[indices.oldIndex].UUID);
        
        // remove the item from the old index in list 1
        _imageTags.Remove(_imageTags[indices.oldIndex]);
    }

    private async Task ListRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 2
        var tag = _list[indices.oldIndex];

        // add it to the new index in list 1
        _imageTags.Insert(indices.newIndex, tag);
        
        await CreateService.AddTagToImage(_assetId, tag.UUID);
       // remove the item from the old index in list 2
       _list.Remove(_list[indices.oldIndex]);
    }
    
    private async Task ImageTagsReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _imageTags[indices.oldIndex];
    
        // Remove from old position
        _imageTags.RemoveAt(indices.oldIndex);
    
        // Insert at new position
        _imageTags.Insert(indices.newIndex, item);
    }

    private void ListReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _list[indices.oldIndex];
    
        // Remove from old position
        _list.RemoveAt(indices.oldIndex);
        
        // Insert at new position
        _list.Insert(indices.newIndex, item);
    }
}
