using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    ///     <MyNamespace:CCanvas/>
    ///
    /// </summary>
    public class CCanvas : Canvas
    {
        private readonly Pen stroke = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)), 0.5);
        private bool drawWF = true;
        public bool DrawWireframe
        {
            get => drawWF;
            set
            {
                if (drawWF != value)
                {
                    drawWF = value;
                    InvalidateVisual();
                }
            }
        }

        static CCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CCanvas), new FrameworkPropertyMetadata(typeof(CCanvas)));
        }

        public CCanvas()
        {
            Loaded += (o, e) =>
            {
                var dc = DataContext as CFrame;
                dc.IsVisibleChanged += (obj, ev) => DrawWireframe = dc.IsVisible;
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (DrawWireframe)
            {
                dc.DrawLine(stroke, new Point(), new Point(Width, Height));
                dc.DrawLine(stroke, new Point(0, Height), new Point(Width, 0));
            }

            base.OnRender(dc);
        }
    }
}
