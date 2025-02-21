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
    

    /// <summary>
    /// Event handler for button clicks that manages view switching based on the button's name.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Event arguments.</param>
    /// <remarks>
    /// This method extracts the view name from the button's Name property by removing "Button" suffix 
    /// and appending "ViewElement". It then finds the corresponding UserControl view and activates it.
    /// The button names must follow the pattern "[Name]Button" and corresponding views must be named 
    /// "[Name]ViewElement".
    /// </remarks>
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
	    if (sender is Button button)
	    {
		    string viewName = button.Name!.Replace("Button", "ViewElement");
		    UserControl? view = this.FindControl<UserControl>(viewName);
		    if (view != null)
		    {
			    SetActiveView(view);
		    }
	    }
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