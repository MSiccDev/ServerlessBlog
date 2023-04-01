using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Behaviors;
namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
	public class ImageEx : Image
	{
		public static readonly BindableProperty TintColorProperty =
			BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(ImageEx), default(Color), propertyChanged: OnTintColorPropertyChanged);

		public ImageEx()
		{

		}

		[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
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

		public Color TintColor
		{
			get => (Color)GetValue(TintColorProperty);
			set => SetValue(TintColorProperty, value);
		}



	}
}

