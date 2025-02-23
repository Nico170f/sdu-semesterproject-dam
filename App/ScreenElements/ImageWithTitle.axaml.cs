using System;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace App.ScreenElements;

/// <summary>
/// ImageWithTitle is a custom control that displays an image with a title and an ID, as well as a button for selecting the product.
/// </summary>
public class ImageWithTitle : TemplatedControl
{

	// This class consists of a few StyledProperty's. This is avalonia's way of creating a bindable property. See https://youtu.be/XmYKOoGlZWo?t=936 for explanation.
	
	
	public static readonly StyledProperty<string> ProductNameProperty = AvaloniaProperty.Register<ImageWithTitle, string>(
		nameof(ProductName),
		defaultValue: "<Product Name>");

	public string ProductName
	{
		get => GetValue(ProductNameProperty);
		set => SetValue(ProductNameProperty, value);
	}

	
	
	public static readonly StyledProperty<string> ProductIDProperty = AvaloniaProperty.Register<ImageWithTitle, string>(
		nameof(ProductID),
		defaultValue: "<Product ID>");

	public string ProductID
	{
		get => GetValue(ProductIDProperty);
		set => SetValue(ProductIDProperty, value);
	}



	public static readonly StyledProperty<string> ProductImageProperty = AvaloniaProperty.Register<ImageWithTitle, string>(
		nameof(ProductImage));

	public string ProductImage
	{
		get => GetValue(ProductImageProperty);
		set => SetValue(ProductImageProperty, value);
	}
	
}