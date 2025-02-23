using System.Collections.Generic;
using App.Logic;
using App.ScreenElements;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace App.Views;

public partial class HomeView : UserControl
{
	
	public List<Product> Products { get; set; } = new List<Product>();
	
	public HomeView ()
	{
		InitializeComponent();
		AddProduct(new Product("Henrik", "1"));
	}
	
	/// <summary>
	/// Adds a product to the products list and updates the grid
	/// </summary>
	/// <param name="product">The product to be added</param>
	public void AddProduct(Product product)
	{
		Products.Add(product);
		UpdateProducts();
	}
	
	/// <summary>
	/// Empties the products list and updates the grid
	/// </summary>
	public void ClearProducts()
	{
		Products.Clear();
		UpdateProducts();
	}
	
	/// <summary>
	/// Updates the products in the grid based on the current products list
	/// </summary>
	private void UpdateProducts()
	{
		// Clear all children
		ProductsContainer.Children.Clear();
		
		// Add all products
		for (int i = 0; i < Products.Count; i++)
		{
			// Handle out of grid bounds
			if (i >= 20)
			{
				break;
			}
			
			Product product = Products[i];
			(int column, int row) = GetGridPosition(i);
			ImageWithTitle imageWithTitle = new ImageWithTitle
			{
				ProductName = product.Name,
				ProductID = product.ID,
				ProductImage = product.ImageURI
			};
			Grid.SetColumn(imageWithTitle, column);
			Grid.SetRow(imageWithTitle, row);
			ProductsContainer.Children.Add(imageWithTitle);
		}
	}
	
	/// <summary>
	/// Calculates the grid position based on the current index, columns and rows
	/// </summary>
	/// <param name="current">The current index</param>
	/// <param name="columns">Amount of columns in the grid. Defaults at 4</param>
	/// <param name="rows">Amount of rows in the grid. Defaults at 5</param>
	/// <returns></returns>
	private (int column, int row) GetGridPosition(int current, int columns = 4, int rows = 5)
	{
		int currentColumn = current % columns;
		int currentRow = current / columns;
		return (currentColumn, currentRow);
	}
	
}