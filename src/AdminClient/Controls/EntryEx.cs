#if IOS || MACCATALYST
using UIKit;
#endif

namespace MSiccDev.ServerlessBlog.AdminClient.Controls
{
    public class EntryEx : Entry
    {
        public EntryEx()
        {
            this.HandlerChanged += OnInstanceHandlerChanged;
            this.HandlerChanging += OnInstanceHandlerChanging;
        }

        private void OnInstanceHandlerChanging(object? sender, HandlerChangingEventArgs e)
        {
            if (sender is Entry entry)
            {
#if IOS || MACCATALYST
                if (entry.Handler?.PlatformView != null)
                    ((UITextField)entry.Handler.PlatformView).BorderStyle = UITextBorderStyle.Bezel;
#endif
            }
        }

        private void OnInstanceHandlerChanged(object? sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
#if IOS || MACCATALYST
                if (entry.Handler?.PlatformView != null)
                    ((UITextField)entry.Handler.PlatformView).BorderStyle = UITextBorderStyle.None;
#endif
            }
        }

    }
}
