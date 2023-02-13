using System;
using CommunityToolkit.Maui.Behaviors;

namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
	public class ShellContentEx : ShellContent
	{
		public static readonly BindableProperty TintColorProperty =
			BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(ShellContentEx), default(Color));


		public Color TintColor
		{
			get => (Color)GetValue(TintColorProperty);
			set => SetValue(TintColorProperty, value);
		}
	}
}

