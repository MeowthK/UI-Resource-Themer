using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UI_Resource_Themer"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UI_Resource_Themer;assembly=UI_Resource_Themer"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CLock/>
    ///
    /// </summary>
    public class CLock : Image
    {
        public event EventHandler LockChanged;
        private bool isLocked = false;
        private readonly BitmapImage[] images =
            {
                new BitmapImage(new Uri(@"pack://application:,,,/LockIcons/unlock.png", UriKind.Absolute)),
                new BitmapImage(new Uri(@"pack://application:,,,/LockIcons/lock.png", UriKind.Absolute))
            };

        public bool IsLocked { get => isLocked;
            set
            {
                if (isLocked != value)
                {
                    isLocked = value;
                    Source = images[isLocked ? 1 : 0];
                    LockChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        static CLock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CLock), new FrameworkPropertyMetadata(typeof(CLock)));
        }

        public CLock()
        {
            Stretch = Stretch.Uniform;
            Loaded += (o, e) => Source = images[isLocked ? 1 : 0];
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            IsLocked = !IsLocked;
            base.OnMouseLeftButtonUp(e);
        }
    }
}
