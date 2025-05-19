using DAM.Presentation.Services;
using DAM.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DAM.Presentation.Components.Pages.Asset_Pages;

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
	
    private List<Asset> _assets = [];
    private string _searchText = "";
    private bool _showTagMenu = false;
    private List<Tag> _allTags = [];
    private HashSet<Tag> _selectedTags = [];

    private int _amount = 20;
    private int _currentPageNumber = 1;
    private int _totalPageCount = 0;
    
    protected override async Task OnInitializedAsync()
    {
        _allTags = (await ReadService.GetTags())?.Tags ?? [];
        UpdateAssetList();
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
	    UpdateAssetList();
    }

    private async void UpdateAssetList ()
    {
	    var response = await ReadService.GetAssets(searchString: _searchText, selectedTags: _selectedTags, amount: _amount, page: _currentPageNumber);
	    
	    if(response is null) return;
		
	    _assets = response.Assets;
	    
	    if (response.TotalCount.HasValue)
	    {
		    int totalAmount = response.TotalCount.Value;
		    _totalPageCount = (int) Math.Ceiling((totalAmount * 1.0f) / _amount);
	    }
	    
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

    private void NextPage ()
    {
	    _currentPageNumber = int.Min(_totalPageCount, _currentPageNumber + 1);
	    UpdateAssetList();
    }

    private void PreviousPage ()
    {
	    _currentPageNumber = int.Max(1, _currentPageNumber - 1);
	    UpdateAssetList();
    }
}