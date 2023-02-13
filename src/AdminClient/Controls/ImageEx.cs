using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Behaviors;

namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
	public class ImageEx : Image
	{
		public static readonly BindableProperty TintColorProperty =
			BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(ImageEx), default(Color), propertyChanged: OnTintColorPropertyChanged);

		public static readonly BindableProperty PlaceholderImageSourceProperty =
			BindableProperty.Create(nameof(PlaceholderImageSource), typeof(ImageSource), typeof(ImageEx), default(ImageSource), propertyChanged: OnPlaceholderImageSourcePropertyChanged);

		public ImageEx()
		{

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		private static void OnTintColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is ImageEx imageEx)
			{
				imageEx.Behaviors.Add(new IconTintColorBehavior()
				{
					TintColor = (Color)newValue
				});
			}
		}

		private static void OnPlaceholderImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is ImageEx imageEx)
			{
				if (newValue != null && imageEx.Source == null)
				{
					imageEx.Source = (ImageSource)newValue;
				}
			}
		}

		public Color TintColor
		{
			get => (Color)GetValue(TintColorProperty);
			set => SetValue(TintColorProperty, value);
		}

		public ImageSource PlaceholderImageSource
		{
			get => (ImageSource)GetValue(PlaceholderImageSourceProperty);
			set => SetValue(PlaceholderImageSourceProperty, value);
		}

		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(this.Source))
			{
				if (this.Source == default(ImageSource))
				{
					this.Source = PlaceholderImageSource;
				}
			}
		}
	}
}

