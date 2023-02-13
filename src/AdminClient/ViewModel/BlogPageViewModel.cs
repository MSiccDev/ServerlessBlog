using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
	public class BlogPageViewModel : ObservableObject
	{
		public BlogPageViewModel()
		{
		}

		public string? BlogName { get; set; }

		public string? Slogan { get; set; }

		public Uri? BlogLogoUrl { get; set; }

		public int PostCount { get; set; }

		public int TagCount { get; set; }

		public int AuthorCount { get; set; }

		public int MediaCount { get; set; }
	}
}

