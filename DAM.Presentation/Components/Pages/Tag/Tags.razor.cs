using DAM.Presentation.Services;
using Microsoft.AspNetCore.Components;

namespace DAM.Presentation.Components.Pages.Tag;

public partial class Tags : ComponentBase
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

	private string _addTagText = "";
	private string _searchText = "";
	private List<Models.Tag> _tags = [];

	private int _amount = 50;
	private int _currentPageNumber = 1;
	private int _totalPageCount = 0;

	protected override async Task OnInitializedAsync()
	{
		UpdateTagList();
	}

	private async void UpdateTagList ()
	{
		(_tags, int totalAmount) = await ReadService.GetTags(searchString: _searchText,amount: _amount,page: _currentPageNumber);
		_totalPageCount = (int)Math.Ceiling((totalAmount * 1.0f)/ _amount);
		StateHasChanged();
	}
	
	private async Task AddTag ()
	{
		if(string.IsNullOrEmpty(_addTagText)) return;
		
		await CreateService.UploadTag(_addTagText);
		UpdateTagList();
	}

	private async Task DeleteTag (Guid tagId)
	{
		await DeleteService.DeleteTag(tagId);
		UpdateTagList();
	}

	private void OnSearchInputChanged(ChangeEventArgs e)
	{
		_searchText = e.Value?.ToString() ?? "";
		UpdateTagList();
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