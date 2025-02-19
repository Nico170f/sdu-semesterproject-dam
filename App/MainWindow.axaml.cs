using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void HomeButton_OnClick(object? sender, RoutedEventArgs e)
    {
	    SetActiveView(HomeViewElement);
    }

    private void SettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
	    SetActiveView(SettingsViewElement);
    }

    /// <summary>
    /// Deactivates all views and activates the view passed as parameter
    /// </summary>
    /// <param name="view"> The view that will get activated </param>
    private void SetActiveView(UserControl view)
    {
	    // set IsVisible of all views/userControls with the class "ViewElement" to false
	    foreach (var child in this.GetVisualDescendants())
	    {
		    if (child is UserControl userControl && userControl.Classes.Contains("ViewElement"))
		    {
			    userControl.IsVisible = false;
		    }
	    }
	    
	    // set IsVisible of the view to true
	    view.IsVisible = true;
    }
}