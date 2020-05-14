using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for CControlsExplorer.xaml
    /// </summary>
    public partial class CControlsExplorer : UserControl
    {
        public CControlsExplorer()
        {
            InitializeComponent();

            Loaded += (o, e) =>
            {
                SearchBox.TextChanged += (obj, ev) =>
                {
                    foreach (Button btn in ControlGroup.Children)
                        btn.Visibility = btn.Content.ToString().ToLower().Contains(SearchBox.Text.ToLower()) ? Visibility.Visible : Visibility.Collapsed;
                };

                CControl.ControlAddedRemoved += (obj, ev) =>
                {
                    ControlGroup.Children.Clear();

                    foreach (var ccontrol in CControl.ControlsActive)
                    {
                        if (ccontrol.Discard)
                            continue;

                        var btn = new Button
                        {
                            DataContext = ccontrol,
                            Padding = new Thickness(3, 0, 3, 0),
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Content = ccontrol.fieldname,
                            Background = new SolidColorBrush(Color.FromRgb(33, 47, 61)),
                            Foreground = new SolidColorBrush(Color.FromRgb(245, 176, 65)),
                            Height = 24,
                            BorderBrush = null,
                        };

                        ccontrol.NameChanged += (obj2, ev2) => btn.Content = ccontrol.fieldname;

                        btn.Click += (obj2, ev2) =>
                        {
                            var focusElement = (obj2 as Button).DataContext as CControl;

                            focusElement.Focus();
                            CControl.LastFocused = focusElement;
                        };

                        ControlGroup.Children.Add(btn);
                    }
                };
            };
        }
    }
}
