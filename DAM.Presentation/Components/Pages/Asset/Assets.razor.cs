using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DAM.Presentation.Components.Pages.Asset;

public partial class Assets : ComponentBase
{

	#region Injects 
	#pragma warning disable CS8618
	[Inject] private NavigationManager Navigation { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	#pragma warning restore CS8618
	#endregion
	
    private List<Guid> _assetsIds = [];
    private string _searchText = "";
    private bool _showTagMenu = false;
    private List<Models.Tag> _allTags = [];
    private HashSet<Guid> _selectedTagIds = []; 
    
    protected override async Task OnInitializedAsync()
    {
        _assetsIds = await ReadService.GetAllAssetIds();
        _allTags = await ReadService.GetAllTags();
    }

    private async Task DeleteAsset (Guid assetId)
    {
	    await DeleteService.DeleteAsset(assetId);
	    UpdateAssetList();
    }

    private void NavigateToHome()
    {
        Navigation.NavigateTo("/dam", true);
    }

    public void NavigateToPage(int pageNum)
    {
        Navigation.NavigateTo($"/dam/assets?Page={pageNum}", true);
    }


    private void ToggleTagMenu()
    {
        _showTagMenu = !_showTagMenu;
    }

    private void OnTagFilterChanged(Guid tagId, bool isChecked)
    {
	    if (isChecked)
	    {
	        _selectedTagIds.Add(tagId);
	    }
	    else
	    {
	        _selectedTagIds.Remove(tagId);
	    }
	    UpdateAssetList();
    }

    private async void UpdateAssetList ()
    {
	    _assetsIds = await ReadService.GetAssetIds(searchText: _searchText, selectedTagIds: _selectedTagIds);
	    StateHasChanged();
    }

    private void OnSearchInputChanged (ChangeEventArgs e)
    {
	    _searchText = e.Value?.ToString() ?? "";
	    UpdateAssetList();
    }

    private async void UploadAsset (InputFileChangeEventArgs e)
    {
	    await CreateService.UploadImage(e);
	    UpdateAssetList();
    }
}