using DAM.Presentation.Services;
using DAM.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Asset_Pages;

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
    
    private List<Tag> _assetTags = [];
    private List<Tag> _tagGallery = [];

    private int _currentPageNumber = 1;
    private int _totalPageCount = 0;
    private int _amount = 50;
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
	    (_tagGallery, int totalAmount) = await ReadService.GetTagsNotOnAsset(_assetId, _searchText, amount: _amount, page: _currentPageNumber);
	    _totalPageCount = (int)Math.Ceiling((totalAmount * 1.0f)/ _amount);
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

    private void NextPage ()
    {
	    _currentPageNumber = int.Min(_totalPageCount, _currentPageNumber + 1);
	    UpdateTagList();
    }

    private void PreviousPage ()
    {
	    _currentPageNumber = int.Max(1, _currentPageNumber - 1);
	    UpdateTagList();
    }
    
}
