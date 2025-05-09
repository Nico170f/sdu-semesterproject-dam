using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Asset;

public partial class Assets : ComponentBase
{
	
	[Inject] private NavigationManager Navigation { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	
    private bool _isLoaded = false;
    private string searchText = "";
    private int size = 20; 
    private int pageNumber = 0;
    private List<string> imageSources = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //await Task.Delay(250);
            _isLoaded = true;
            StateHasChanged();
        }
    }

    public async Task SearchButton()
    {
        imageSources = await ReadService.GetAssetIdsBySearching(size, pageNumber, searchText);
    }
    
    public void OnImageClickNavigateTo(string imageUrl)
    {
        //http://localhost:5115/api/v1/assets/GetImageByUUID?uuid=008bd3e2-5a4d-4a16-ae8b-f7537bee8642
        string imageId = imageUrl.Substring(imageUrl.Length - 36); //ew
        Navigation.NavigateTo($"/dam/assets/edit?imageId={imageId}");
    }
    
    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(250);

        var uri = new Uri(Navigation.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("Page", out var pageNumString))
            pageNumber = int.Parse(pageNumString);

        foreach (var assetId in await ReadService.GetAssetIdsBySearching(size, pageNumber, searchText))
        {
	        imageSources.Add(await ReadService.GetAssetContentById(assetId));
        }
        
        allTags = await ReadService.GetAllTags();
        
    }

    public void NavigateToHome()
    {
        Navigation.NavigateTo("/dam", true);
    }

    public void NavigateToPage(int pageNum)
    {
        Navigation.NavigateTo($"/dam/assets?Page={pageNum}", true);
    }
        
    private bool showTagMenu = false;
    private List<Models.Tag> allTags = new();
    private HashSet<string> selectedTags = new(); 

    public void ToggleTagMenu()
    {
        showTagMenu = !showTagMenu;
    }

    public void OnTagFilterChanged(string tag, bool isChecked)
    {
        if (isChecked)
            selectedTags.Add(tag);
        else
            selectedTags.Remove(tag);
    }

    public async void ApplyFilter()
    {
        imageSources = await ReadService.GetAssetsByTags(selectedTags.ToList());
        StateHasChanged();
    }
	
}