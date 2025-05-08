using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Tag;

public partial class Tags : ComponentBase
{
	
	[Inject] private NavigationManager Navigation { get; set; }
	[Inject] private CreateService CreateService { get; set; }
	[Inject] private ReadService ReadService { get; set; }
	[Inject] private UpdateService UpdateService { get; set; }
	[Inject] private DeleteService DeleteService { get; set; }
	
	private bool DEBUG = true;

	private string addTagText = "";
	private string searchText = "";
	private List<Models.Tag> tags = [];

	public async Task SearchButton()
	{
		tags = await ReadService.GetAllTags();

		if (!string.IsNullOrWhiteSpace(searchText))
		{
			tags = tags
				.Where(tag => tag.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
				.ToList();
		}
	}


	public async Task DeleteTag (Guid TagId)
	{
		await DeleteService.DeleteTag(TagId.ToString());
		tags = await ReadService.GetAllTags();
	}

	public async Task AddTag (string name)
	{
		// check if a tag with that name already exists and return early if true
		if (tags.Any(tag => tag.Name.ToLower() == name.ToLower()))
		{
			return;
		}
        
		// check if the name is empty
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
        
		// check if there already exists a tag with that name in the local list
		var existingTag = tags.FirstOrDefault(tag => tag.Name.ToLower() == name.ToLower());
		if (existingTag != null)
		{
			return;
		}

		await CreateService.UploadTag(name);
		
		tags = await ReadService.GetAllTags();

	}
	protected override async Task OnInitializedAsync()
	{
    
		tags = await ReadService.GetAllTags();
	}

	public void NavigateToHome()
	{
		Navigation.NavigateTo("/", true);
	}
	
}