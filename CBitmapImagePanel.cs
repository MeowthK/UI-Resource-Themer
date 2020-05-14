using System;
using System.Runtime.CompilerServices;
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
    ///     <MyNamespace:CBitmapImagePanel/>
    ///
    /// </summary>
    public class CBitmapImagePanel : CControl
    {
        private string img = string.Empty;
        private ImageSource imgsrc = null;
        private string imgcolor = "0 0 0 0";
        private string scaleimg = "1";

        public string scaleimage
        {
            get => scaleimg;
            set
            {
                var trimmedVal = value.Trim();

                if (scaleimg != trimmedVal)
                {
                    scaleimg = trimmedVal;

                    if (imgsrc != null)
                        InvalidateVisual();
                }
            }
        }

        public string image
        {
            get => img;
            set
            {
                // Check if in design mode or old value is the same as the new value, discard if true
                if (Util.DesignMode || img == value.Trim())
                    return;

                var temp = value.Trim();
                imgsrc = Util.ConvertTGAToPNG(temp);
                InvalidateVisual();
                img = temp;
            }
        }

        public string imagecolor { get => imgcolor; set { imgcolor = value; InvalidateVisual(); } }

        static CBitmapImagePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CBitmapImagePanel), new FrameworkPropertyMetadata(typeof(CBitmapImagePanel)));
        }

        public CBitmapImagePanel()
        {
            Unloaded += (o, e) =>
            {
                imgsrc = null;
                GC.Collect();
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (imgsrc != null)
            {
                var rect = new Rect(RenderSize);

                drawingContext.DrawRectangle(new SolidColorBrush(Util.StringToColor(imgcolor)), Util.EmptyPen, rect);

                rect.X = 0;
                rect.Y = 0;
                rect.Width = scaleimage != "0" ? Width : imgsrc.Width;
                rect.Height = scaleimage != "0" ? Height : imgsrc.Height;
                drawingContext.DrawImage(imgsrc, rect);
            }

            base.OnRender(drawingContext);
        }
    }
}
