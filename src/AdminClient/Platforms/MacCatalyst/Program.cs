using UIKit;
namespace AdminClient;

public class Program
{
	// This is the main entry point of the application.
	static void Main(string[] args)
	{
		//TODO:
		//HACK Rider issue
		//https://youtrack.jetbrains.com/issue/RIDER-79838
		Thread.Sleep(3000);
		//HACK Rider issue
		
		
		// if you want to use a different Application Delegate class from "AppDelegate"
		// you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
