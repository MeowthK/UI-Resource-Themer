using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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
    ///     <MyNamespace:CFrame/>
    ///
    /// </summary>
    public class CFrame : CControl
    {
        private readonly List<UIElement> children = new List<UIElement>();

        public UIElement[] Children { get { return children.ToArray(); } }

        public CCanvas Content { get; } = new CCanvas();

        public void AddChild(UIElement child)
        {
            if (children.Contains(child))
                return;

            Content.Children.Add(child);
            children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (children.Contains(child))
            {
                Content.Children.Remove(child);
                children.Remove(child);
            }
        }

        public void ClearChildren()
        {
            Content.Children.Clear();
            children.Clear();
        }

        static CFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CFrame), new FrameworkPropertyMetadata(typeof(CFrame)));
        }

        public CFrame()
        {
            BoxColor = Color.FromArgb(100, 255, 255, 255);
            //Content.Visibility = Visibility.Hidden;

            Loaded += (o, e) =>
            {
                var parent = Parent as Canvas;
                parent.Children.Add(Content);

                Canvas.SetLeft(Content, GetActualX);
                Canvas.SetTop(Content, GetActualY);

                Content.DataContext = this;

                Moved += (obj, ev) =>
                {
                    //if (Content.Visibility == Visibility.Hidden)
                    //    Content.Visibility = Visibility.Visible;

                    Canvas.SetLeft(Content, GetActualX);
                    Canvas.SetTop(Content, GetActualY);
                };

                //IsEnabledChanged += (oj, ev) => Content.IsEnabled = IsEnabled;
                //IsVisibleChanged += (obj, ev) =>
                //{
                //    Content.DrawWireframe = Visibility == Visibility.Visible;
                //};
                //NameChanged += (obj, ev) => InvalidateVisual();
            };

            Unloaded += (o, e) =>
            {
                Content.Children.Clear();

                if ((Content.Parent as Canvas) != null)
                    (Content.Parent as Canvas).Children.Remove(Content);

                GC.Collect();
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            //if (fieldname == "ViewPortBackGround")
            //{
            //    var mx = 20;
            //    var my = 30;

            //    var rect = new Rect(RenderSize)
            //    {
            //        X = mx,
            //        Y = my
            //    };

            //    rect.Width = rect.Width - mx * 2 < 0 ? 0 : rect.Width - mx * 2;
            //    rect.Height = rect.Height - my * 2 < 0 ? 0 : rect.Height - my * 2;

            //    drawingContext.DrawRectangle(Util.DefaultBG, Util.EmptyPen, rect);

            //    rect.X = 30;
            //    rect.Y = 20;

            //    rect.Width = rect.Width - 20 < 0 ? 0 : rect.Width - 20;

            //    var th = rect.Height;
            //    rect.Height = 10;

            //    drawingContext.DrawRectangle(Util.DefaultBG, Util.EmptyPen, rect);

            //    rect.Y = th + 30;
            //    drawingContext.DrawRectangle(Util.DefaultBG, Util.EmptyPen, rect);
            //}
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Content.Width = Width;
            Content.Height = Height;

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
