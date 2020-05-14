using System.Windows;
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
    ///     <MyNamespace:CImagePanel/>
    ///
    /// </summary>
    public class CImagePanel : CBitmapImagePanel
    {
        private string fillclr = "0 0 0 0";
        private Brush brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        public string fillcolor { get => fillclr;
            set
            {
                if (fillclr != value)
                {
                    fillclr = value;
                    brush.SetValue(SolidColorBrush.ColorProperty, Util.StringToColor(fillclr));
                    InvalidateVisual();
                }
            }
        }

        static CImagePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CImagePanel), new FrameworkPropertyMetadata(typeof(CImagePanel)));
        }

        public CImagePanel()
        {
            scaleimage = "0";
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(brush, Util.EmptyPen, new Rect(RenderSize));
            base.OnRender(drawingContext);
        }
    }
}
