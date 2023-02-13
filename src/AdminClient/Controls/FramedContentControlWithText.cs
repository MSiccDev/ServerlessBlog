namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
    public class FramedContentControlWithText : Grid
    {
        private readonly Frame _rootFrame;
        private readonly Label _frameTextLabel;


        public static BindableProperty BorderColorProperty =>
            BindableProperty.Create(nameof(FramedContentControlWithText.BorderColor), typeof(Color), typeof(FramedContentControlWithText), propertyChanged: OnBorderColorPropertyChanged);

        public static BindableProperty TextColorProperty =>
            BindableProperty.Create(nameof(FramedContentControlWithText.TextColor), typeof(Color), typeof(FramedContentControlWithText), propertyChanged: OnTextColorPropertyChanged);

        public static BindableProperty TextSizeProperty =>
            BindableProperty.Create(nameof(FramedContentControlWithText.TextSize), typeof(double), typeof(FramedContentControlWithText), 18, propertyChanged: OnTextSizePropertyChanged);

        public static BindableProperty TextProperty =>
            BindableProperty.Create(nameof(FramedContentControlWithText.Text), typeof(string), typeof(FramedContentControlWithText), propertyChanged: OnTextPropertyChanged);

        public static BindableProperty ContentProperty =>
            BindableProperty.Create(nameof(FramedContentControlWithText.Content), typeof(Microsoft.Maui.Controls.View), typeof(FramedContentControlWithText), propertyChanged: OnContentPropertyChanged);


        public FramedContentControlWithText()
        {
            _rootFrame = new Frame();

            _frameTextLabel = new Label
            {
                Margin = new Thickness(6, -16, 12, 0),
                Padding = new Thickness(6, 0),
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                Text = "PLEASE SET A TEXT VIA THE TEXT PROPERTY"
            };

            _rootFrame.SetAppThemeColor(BackgroundColorProperty, Colors.Black, Colors.Black);
            _rootFrame.SetAppThemeColor(Microsoft.Maui.Controls.Frame.BorderColorProperty, Colors.Black, Colors.White);

            _frameTextLabel.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
            _frameTextLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            this.Children.Add(_rootFrame);
            this.Children.Add(_frameTextLabel);

        }

        private static void OnBorderColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FramedContentControlWithText current)
            {
                if (newValue is not default(Color))
                {
                    current._rootFrame.BorderColor = (Color)newValue;
                    current._frameTextLabel.TextColor = (Color)newValue;
                }
            }
        }

        private static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FramedContentControlWithText current)
            {
                if (newValue is not default(Color))
                    current._frameTextLabel.TextColor = (Color)newValue;
            }
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

        public Color BorderColor
        {
            get => (Color)GetValue(FramedContentControlWithText.BorderColorProperty);
            set => SetValue(FramedContentControlWithText.BorderColorProperty, value);
        }


        public Color TextColor
        {
            get => (Color)GetValue(FramedContentControlWithText.TextColorProperty);
            set => SetValue(FramedContentControlWithText.TextColorProperty, value);
        }


        public double TextSize
        {
            get => (double)GetValue(FramedContentControlWithText.TextSizeProperty);
            set => SetValue(FramedContentControlWithText.TextSizeProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(FramedContentControlWithText.TextProperty);
            set => SetValue(FramedContentControlWithText.TextProperty, value);
        }

        public Microsoft.Maui.Controls.View Content
        {
            get => (Microsoft.Maui.Controls.View)GetValue(FramedContentControlWithText.ContentProperty);
            set => SetValue(FramedContentControlWithText.ContentProperty, value);
        }
    }
}
