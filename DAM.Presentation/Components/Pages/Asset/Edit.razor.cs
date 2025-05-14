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
    
    private List<Models.Tag> _assetTags = [];
    private List<Models.Tag> _tagGallery = [];
        
    protected override async Task OnInitializedAsync ()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("assetId", out var id))
	        _assetId = new Guid(id);

        _assetTags = await ReadService.GetTagsByAsset(_assetId);

        UpdateTagList();

    }

    private void OnSearchInputChanged (ChangeEventArgs e)
    {
	    _searchText = e.Value?.ToString() ?? "";
	    UpdateTagList();
    }

    private async void UpdateTagList ()
    {
	    _tagGallery = await ReadService.GetTagsNotOnAsset(_assetId, _searchText);
	    StateHasChanged();
    }
    
    private async Task ImageTagsRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 1
        var item = _assetTags[indices.oldIndex];

        // add it to the new index in list 2
        _tagGallery.Insert(indices.newIndex, item);

        await DeleteService.RemoveTagFromAsset(_assetId, _assetTags[indices.oldIndex].UUID);
        
        // remove the item from the old index in list 1
        _assetTags.Remove(_assetTags[indices.oldIndex]);
    }

    private async Task ListRemove((int oldIndex, int newIndex) indices)
    {
        // get the item at the old index in list 2
        var tag = _tagGallery[indices.oldIndex];

        // add it to the new index in list 1
        _assetTags.Insert(indices.newIndex, tag);
        
        await CreateService.AddTagToImage(_assetId, tag.UUID);
       // remove the item from the old index in list 2
       _tagGallery.Remove(_tagGallery[indices.oldIndex]);
    }
    
    private async Task ImageTagsReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _assetTags[indices.oldIndex];
    
        // Remove from old position
        _assetTags.RemoveAt(indices.oldIndex);
    
        // Insert at new position
        _assetTags.Insert(indices.newIndex, item);
    }

    private void ListReorder((int oldIndex, int newIndex) indices)
    {
        // Get the item being moved
        var item = _tagGallery[indices.oldIndex];
    
        // Remove from old position
        _tagGallery.RemoveAt(indices.oldIndex);
        
        // Insert at new position
        _tagGallery.Insert(indices.newIndex, item);
    }
}
