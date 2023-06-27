using System;
using System.Globalization;

namespace MSiccDev.ServerlessBlog.AdminClient.Converters
{
	public class UriImageSourceWithPlaceholderConverter : IValueConverter
	{
		public ImageSource? PlaceholderImageSource { get; set; }

		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Uri imageUri)
				return ImageSource.FromUri(imageUri);

			return PlaceholderImageSource;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

