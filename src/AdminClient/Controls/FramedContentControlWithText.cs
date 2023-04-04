

using System.Runtime.CompilerServices;

namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
    public class FramedContentControlWithText : Grid
    {
        private readonly Frame _rootFrame;
        private readonly Label _frameTextLabel;


        public static BindableProperty TextSizeProperty =>
            BindableProperty.Create(nameof(TextSize), typeof(double), typeof(FramedContentControlWithText), 18, propertyChanged: OnTextSizePropertyChanged);

        public static BindableProperty TextProperty =>
            BindableProperty.Create(nameof(Text), typeof(string), typeof(FramedContentControlWithText), propertyChanged: OnTextPropertyChanged);

        public static BindableProperty ContentProperty =>
            BindableProperty.Create(nameof(Content), typeof(Microsoft.Maui.Controls.View), typeof(FramedContentControlWithText), propertyChanged: OnContentPropertyChanged);


        public FramedContentControlWithText()
        {
            _rootFrame = new Frame() { BackgroundColor = Colors.Transparent, ZIndex = 1 };

			if (App.Current?.Resources.ContainsKey("Ios_OpaqueSeparator") ?? false)
				_rootFrame.BorderColor = (Color)App.Current.Resources["Ios_OpaqueSeparator"];

			_frameTextLabel = new Label
            {
                Margin = new Thickness(6, -16, 12, 0),
                Padding = new Thickness(6, 0),
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                Text = "PLEASE SET A TEXT VIA THE TEXT PROPERTY",
                ZIndex = 2
            };

			if (App.Current?.Resources.ContainsKey("Ios_SystemLabel") ?? false)
				_frameTextLabel.TextColor = (Color)App.Current.Resources["Ios_SystemLabel"];


			this.Children.Add(_rootFrame);
            this.Children.Add(_frameTextLabel);

        }

        private static void OnTextSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FramedContentControlWithText current)
            {
                //NamedSize is deprecated, so I let MAUI throw its exception here.
                //Otherwise, allow values between 18 and 24

                double doubleValue = (double)newValue;

                if (doubleValue < 18)
                    doubleValue = 18;
                if (doubleValue > 24)
                    doubleValue = 24;

                current._frameTextLabel.FontSize = doubleValue;

            }
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FramedContentControlWithText current)
            {
                current._frameTextLabel.Text = newValue?.ToString();
            }
        }

        private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FramedContentControlWithText current)
            {
                current._rootFrame.Content = (Microsoft.Maui.Controls.View)newValue;
            }
        }

        public double TextSize
        {
            get => (double)GetValue(TextSizeProperty);
            set => SetValue(TextSizeProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Microsoft.Maui.Controls.View Content
        {
            get => (Microsoft.Maui.Controls.View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
		}
	}
}
