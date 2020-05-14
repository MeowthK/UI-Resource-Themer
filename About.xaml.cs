using System;
using System.Diagnostics;
using System.Windows;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            Topmost = true;
            Loaded += (o, e) => Deactivated += (obj, ev) =>
            {
                try { Close(); }
                catch (InvalidOperationException) { }
            };
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
