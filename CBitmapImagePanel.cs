using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        private string imgcolor = "0 0 0 255";
        private string scaleimg = "1";
        private string type;
        //private static readonly BitmapImage defaultImage = new BitmapImage(new Uri(@"pack://application:,,,/defaultimg.png", UriKind.Absolute));

        public ImageSource ImageSource { get; private set; } = null;

        public string scaleimage
        {
            get => scaleimg;
            set
            {
                var trimmedVal = value.Trim();

                if (scaleimg != trimmedVal)
                {
                    scaleimg = trimmedVal;

                    if (ImageSource != null)
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

                img = value.Trim();
                var imgtemp = Util.ConvertTGAToPNG(img);

                if (imgtemp == null)
                {
                    //imgtemp = defaultImage;
                    if (type == "CBitmapImagePanel")
                        OpacityMask = null;
                    ImageSource = null;

                    InvalidateVisual();
                    return;
                    //imgtemp = 
                    //OpacityMask = null;
                    //Background = Brushes.Black;
                    //LastFocused = null;
                    //return;
                }

                ImageSource = imgtemp;

                if (type == "CBitmapImagePanel")
                    if (ImageSource != null)
                        OpacityMask = new ImageBrush(ImageSource);

                ImageSource.Freeze();

                InvalidateVisual();
            }
        }

        public string imagecolor { get => imgcolor;
            set
            {
                if (imgcolor != value)
                {
                    imgcolor = value;

                    if (imgcolor.ToLower() == "titleicon")
                        Background = Util.DefaultFG;
                    else
                        Background = new SolidColorBrush(Util.StringToColor(imgcolor));

                    InvalidateVisual();
                }
            }
        }

        static CBitmapImagePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CBitmapImagePanel), new FrameworkPropertyMetadata(typeof(CBitmapImagePanel)));
        }

        public CBitmapImagePanel()
        {
            type = GetType().Name;

            Background = Brushes.Black;
            //ImageSource = defaultImage;

            if (type == "CBitmapImagePanel")
                OpacityMask = new ImageBrush(ImageSource);

            //Loaded += (o, e) => 

            Unloaded += (o, e) =>
            {
                if (IsDeleted)
                    ImageSource = null;
            };
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (type == "CBitmapImagePanel")
            {
                OpacityMask = null;
                Background = Brushes.Transparent;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (type == "CBitmapImagePanel")
            {
                OpacityMask = new ImageBrush(ImageSource ?? null);

                if (imgcolor.ToLower() == "titleicon")
                    Background = Util.DefaultFG;
                else
                    Background = new SolidColorBrush(Util.StringToColor(imgcolor));
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (type == "CBitmapImagePanel")
            {
                if (ImageSource != null)
                    drawingContext.DrawImage(ImageSource, new Rect(RenderSize));
            }

            base.OnRender(drawingContext);
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    if (type == "CBitmapImagePanel")
        //        Background = IsFocused ? SelectionBrush : new SolidColorBrush(bgcolor);
        //    //if (ImageSource != null)
        //    //{
        //    //    //var rect = new Rect(RenderSize)
        //    //    //{
        //    //    //    //drawingContext.DrawRectangle(new SolidColorBrush(Util.StringToColor(imgcolor)), Util.EmptyPen, rect);

        //    //    //    X = 0,
        //    //    //    Y = 0,
        //    //    //    Width = scaleimage != "0" ? Width : imgsrc.Width,
        //    //    //    Height = scaleimage != "0" ? Height : imgsrc.Height
        //    //    //};

        //    //    //drawingContext.DrawImage(imgsrc, rect);

        //    //    ////if (!imgsrc.IsFrozen)
        //    //    ////    imgsrc.Freeze();
        //    //}

        //    base.OnRender(drawingContext);
        //}
    }
}
