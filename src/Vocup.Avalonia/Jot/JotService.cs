using Avalonia.Controls;
using Jot;

namespace Vocup.Avalonia.Jot
{
    /// <summary>
    /// Static bootstrap for the Jot objects
    /// </summary>
    static class JotService
    {

        public static Tracker Tracker = new Tracker();

        static JotService()
        {
            // configure tracking for all Form objects

            Tracker
                .Configure<Window>()
                .Id(f => f.Name)
                .Properties(f => new { f.Width, f.Height, f.Position, f.WindowState })
                .PersistOn(nameof(Window.Closing))
                //.WhenPersistingProperty((f, p) => p.Cancel = (f.WindowState != WindowState.Normal && (p.Property == nameof(Form.Height) || p.Property == nameof(Form.Width) || p.Property == nameof(Form.Top) || p.Property == nameof(Form.Left))))
                .StopTrackingOn(nameof(Window.Closed));
        }
    }
}
