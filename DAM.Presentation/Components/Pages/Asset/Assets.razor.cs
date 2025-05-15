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
	
    private List<Guid> _assetIds = [];
    private string _searchText = "";
    private bool _showTagMenu = false;
    private List<Models.Tag> _allTags = [];
    private HashSet<Guid> _selectedTagIds = [];

    private int _amount = 20;
    private int _currentPageNumber = 1;
    private int _totalPageCount = 0;
    
    protected override async Task OnInitializedAsync()
    {
        _allTags = await ReadService.GetAllTags();
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
	    (_assetIds, int totalAmount) = await ReadService.GetAssetIds(searchString: _searchText, selectedTags: _selectedTagIds, amount: _amount, page: _currentPageNumber);
	    _totalPageCount = (int)Math.Ceiling((totalAmount * 1.0f)/ _amount);
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