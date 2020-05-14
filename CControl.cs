using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    ///     <MyNamespace:CControl/>
    ///
    /// </summary>
    public class CControl : Control
    {
        #region Generic CS Control Properties
        public event EventHandler Moved, NameChanged;
        public Canvas OldParent { get; set; } = null;

        private static List<CControl> ctlsActive = new List<CControl>();

        public static CControl[] ControlsActive
        {
            get
            {
                return ctlsActive.ToArray();
            }
        }

        public string fieldname { get => actualname;
            set {
                if (actualname != value)
                {
                    actualname = value;
                    NameChanged?.Invoke(this, EventArgs.Empty);
                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool Discard { get; set; } = false;
        public bool Locked { get; set; } = false;

        public List<string> UnknownAttrs = new List<string>();

        public string GetUnknownAttrs
        {
            get
            {
                var ret = string.Empty;

                foreach (var attr in UnknownAttrs)
                    ret += "\t\t" + attr + "\n";

                return ret;
            }
        }

        public string headerName { get; set; }

        public string enabled { get => IsEnabled ? "1" : "0";
            set => IsEnabled = value.Trim() == "1"; }
        public string visible
        {
            get => IsVisible ? "1" : "0";
            set => Visibility = value.Trim() == "1" ? Visibility.Visible : Visibility.Hidden; }

        public string xpos
        {
            get => x;
            //{
            //    return x;

            //    xval = (int)Canvas.GetLeft(this);

            //    if (x.StartsWith("c") || x.StartsWith("r"))
            //        return x;

            //    return Canvas.GetLeft(this) + "";
            //}
            set
            {
                if (x != value)
                {
                    x = value.ToLower();

                    //if (x.Contains('c') || x.Contains('r'))
                    //{
                    //    if (Parent is CCanvas)
                    //    {
                    //        (Parent as CCanvas).Children.Remove(this);

                    //        if (!Util.OriginalParent.Children.Contains(this))
                    //        {
                    //            Util.OriginalParent.Children.Add(this);
                    //            Visibility = Visibility.Visible;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (Parent != OldParent)
                    //    {
                    //        Util.OriginalParent.Children.Remove(this);

                    //        if (!OldParent.Children.Contains(this))
                    //        {
                    //            OldParent.Children.Add(this);
                    //            Visibility = Visibility.Visible;
                    //        }
                    //    }
                    //}

                    Canvas.SetLeft(this, Util.GetOffset(this, x));
                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                    Moved?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public string ypos
        {
            get => y;
            //{
            //    yval = (int)Canvas.GetTop(this);

            //    if (y.StartsWith("c"))
            //        return y;

            //    return Canvas.GetTop(this) + "";
            //}
            set
            {
                if (y != value)
                {
                    y = value.ToLower();
                    Canvas.SetTop(this, Util.GetOffset(this, y, false));
                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                    Moved?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int GetActualX { get { return (int)Canvas.GetLeft(this); } }
        public int GetActualY { get { return (int)Canvas.GetTop(this); } }

        public string wide
        {
            get => w;
            set
            {
                if (w != value)
                {
                    w = value;

                    var successw = int.TryParse(value, out int newWidth);
                    Width = successw? newWidth : 0;

                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string tall
        {
            get => h;
            set
            {
                if (h != value)
                {
                    h = value;

                    var successh = int.TryParse(value, out int newHeight);
                    Height = successh ? newHeight : 0;

                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        //public double wide { get => Width; set { Width = value; CSControlChanged?.Invoke(this, EventArgs.Empty); } }
        //public double tall { get => Height; set { Height = value; CSControlChanged?.Invoke(this, EventArgs.Empty); } }
        #endregion

        public string controlname { get; set; }

        #region Private Fields
        private string x = "", y = "", w = "", h = "", actualname;
        private Color boxColor = Color.FromArgb(0, 0, 0, 0);
        private static CControl lastFocused;
        #endregion

        #region Public Fields
        public Color BoxColor { get => boxColor; set { boxColor = value; InvalidateVisual(); } }
        public Point LastPoint { get; private set; }
        public static CControl LastFocused
        {
            get => lastFocused;
            set
            {
                if (lastFocused != value)
                {
                    lastFocused = value;
                    LastFocusedChanged?.Invoke(lastFocused, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler LastFocusedChanged, CSControlChanged, ControlAddedRemoved;
        #endregion

        static CControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CControl), new FrameworkPropertyMetadata(typeof(CControl)));
        }

        public CControl()
        {
            actualname = Name;
            controlname = Util.GetControlName(GetType());

            Focusable = true;
            IsTabStop = false;

            ClipToBounds = true;

            wide = "64";
            tall = "24";

            Loaded += (o, e) =>
            {
                ctlsActive.Add(this);
                ControlAddedRemoved?.Invoke(this, EventArgs.Empty);

                //LastFocused = this;
                LastPoint = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
                LastFocusedChanged += (obj, ev) => InvalidateVisual();

                //OldParent. += (obj, ev) => MessageBox.Show("!!!");
            };

            Unloaded += (o, e) =>
            {
                ctlsActive.Remove(this);
                ControlAddedRemoved?.Invoke(this, EventArgs.Empty);
            };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            CSControlChanged?.Invoke(this, EventArgs.Empty);

            if (e.Property.Name == "IsFocused")
            {
                if (IsFocused)
                    LastFocused = this;

                InvalidateVisual();
            }

            base.OnPropertyChanged(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (IsEnabled && e.LeftButton == MouseButtonState.Pressed)
            {
                Focus();
                LastFocused = this;
                LastPoint = e.GetPosition(this);
            }

            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
               LastFocused = null;
               InvalidateVisual();

                e.Handled = true;
                return;
            }

            if (!IsEnabled || Locked)
            {
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Delete:
                    if (LastFocused == null)
                        break;

                    var parent = Parent;

                    if (parent != null && parent is Canvas)
                    {
                        (parent as Canvas).Children.Remove(this);
                        LastFocused = null;
                    }

                    break;

                case Key.Left:
                    if (!xpos.StartsWith("c") && !xpos.StartsWith("r"))
                        xpos = (GetActualX - 1) + "";
                    break;

                case Key.Right:
                    if (!xpos.StartsWith("c") && !xpos.StartsWith("r"))
                        xpos = (GetActualX + 1) + "";
                    break;

                case Key.Up:
                    if (!ypos.StartsWith("c"))
                        ypos = (GetActualY - 1) + "";
                    break;

                case Key.Down:
                    if (!ypos.StartsWith("c"))
                        ypos = (GetActualY + 1) + "";
                    break;
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var pen = new Pen(new SolidColorBrush(boxColor), 1);
            var brush = Brushes.Transparent;

            if (LastFocused != null && (LastFocused == this || IsFocused))
            {
                pen.Brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                brush = new SolidColorBrush(Color.FromArgb(100, 93, 173, 226));
            }

            var rect = new Rect(RenderSize);
            drawingContext.DrawRectangle(brush, pen, rect);
        }
    }
}
