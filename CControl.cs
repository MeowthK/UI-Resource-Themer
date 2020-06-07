using System;
using System.Collections.Generic;
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

        private static readonly HashSet<CControl> ctlsActive = new HashSet<CControl>();

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

        public bool IsDeleted { get; set; } = false;
        public bool LockX { get; set; } = false;
        public bool LockY { get; set; } = false;
        public bool LockWidth { get; set; } = false;
        public bool LockHeight { get; set; } = false;

        public bool Discard { get; set; } = false;
        public bool Locked { get; set; } = false;
        public bool IsMultiSelected
        {
            get => ismultiselected;
            set
            {
                ismultiselected = value;
                InvalidateVisual();
            }
        }

        public List<string> UnknownAttrs = new List<string>();

        public CControl GetClone
        {
            get
            {
                CControl ctl = (CControl) Activator.CreateInstance(GetType());

                ctl.controlname = controlname;
                ctl.Discard = Discard;
                ctl.OldParent = OldParent;
                ctl.Locked = false;
                ctl.enabled = "1";
                ctl.visible = "1";
                ctl.wide = wide;
                ctl.tall = tall;

                return ctl;
            }
        }

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

        private string visible_P = "1";
        private string enabled_P = "1";

        public string headerName { get; set; }

        public string enabled
        {
            get => enabled_P;
            set
            {
                if (enabled_P != value)
                {
                    enabled_P = value;
                    IsEnabled = enabled_P == "1";
                }
            }
        }

        public string visible
        {
            get => visible_P;
            set
            {
                if (visible_P != value)
                {
                    visible_P = value;
                    Visibility = visible_P == "1" ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

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
                    Width = successw ? (newWidth >= 0 ? newWidth : 0) : 0;

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
                    Height = successh ? (newHeight >= 0 ? newHeight : 0) : 0;

                    CSControlChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        //public double wide { get => Width; set { Width = value; CSControlChanged?.Invoke(this, EventArgs.Empty); } }
        //public double tall { get => Height; set { Height = value; CSControlChanged?.Invoke(this, EventArgs.Empty); } }
        #endregion

        public string controlname { get; set; }

        #region Private Fields
        private bool ismultiselected = false, isMouseDown = false;
        private string x = "", y = "", w = "", h = "", actualname;
        private Color boxColor = Color.FromArgb(0, 0, 0, 0);
        private static CControl lastFocused;
        #endregion

        #region Public Fields
        public Color BoxColor { get => boxColor; set { boxColor = value; InvalidateVisual(); } }
        public Point LastPoint { get; private set; }
        public Point LastParentPoint { get; private set; }
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

        //~CControl()
        //{
        //    GC.Collect();
        //}

        public static void ClearActiveControls()
        {
            foreach (var cc in ctlsActive)
                cc.IsDeleted = true;

            ctlsActive.Clear();
        }

        public CControl()
        {
            actualname = Name;
            controlname = Util.GetControlName(GetType());

            Focusable = true;
            IsTabStop = false;

            ClipToBounds = true;

            Loaded += Load;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var xtemp = xpos;
            xpos = "0";
            xpos = xtemp;

            ctlsActive.Add(this);
            ControlAddedRemoved?.Invoke(this, EventArgs.Empty);

            //LastFocused = this;
            LastPoint = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            LastFocusedChanged += UpdateVisual;
            Unloaded += Unload;
        }

        private void UpdateVisual(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void Unload(object sender, RoutedEventArgs e)
        {
            if (IsDeleted)
            {
                if (Util.SelectedControls.Contains(this))
                    Util.SelectedControls.Remove(this);

                if (LastFocused == this)
                    LastFocused = null;

                ctlsActive.Remove(this);
                ControlAddedRemoved?.Invoke(this, EventArgs.Empty);

                Loaded -= Load;
                LastFocusedChanged -= UpdateVisual;
                Unloaded -= Unload;

                //GC.Collect();
            }
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

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                isMouseDown = false;

                Focus();
                LastFocused = this;

                if (!Util.IsCtrlUp)
                {
                    //if (this is CFrame)
                    //{
                    //    if ((this as CFrame).Content == Util.OutermostParent)
                    //        return;
                    //}

                    IsMultiSelected = !IsMultiSelected;

                    if (!ismultiselected)
                    {
                        if (Util.SelectedControls.Contains(this))
                            Util.SelectedControls.Remove(this);

                        LastFocused = null;
                    }
                    else
                        Util.SelectedControls.Add(this);
                }
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isMouseDown = true;
                LastPoint = e.GetPosition(this);
                LastParentPoint = e.GetPosition(Util.OriginalParent);
            }

            base.OnMouseDown(e);
        }

        //protected override void OnMouseDown(MouseButtonEventArgs e)
        //{
        //    if (IsEnabled && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Focus();
        //        LastFocused = this;
        //        LastPoint = e.GetPosition(this);

        //        if (!Util.IsCtrlUp)
        //        {
        //            if (this is CFrame)
        //            {
        //                if ((this as CFrame).Content == Util.OutermostParent)
        //                    return;
        //            }

        //            IsMultiSelected = !IsMultiSelected;

        //            if (!ismultiselected)
        //            {
        //                if (Util.SelectedControls.Contains(this))
        //                    Util.SelectedControls.Remove(this);

        //                LastFocused = null;
        //            }
        //            else
        //                Util.SelectedControls.Add(this);
        //        }
        //    }

        //    base.OnMouseDown(e);
        //}

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
               LastFocused = null;
               InvalidateVisual();

                foreach (var cc in Util.SelectedControls)
                    cc.IsMultiSelected = false;
                Util.SelectedControls.Clear();

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

                    foreach (var cc in Util.SelectedControls)
                    {
                        var parent = cc.Parent;

                        if (parent != null && parent is Canvas)
                        {
                            if (cc is CFrame && (cc as CFrame).Content == Util.OutermostParent)
                                continue;

                            (parent as Canvas).Children.Remove(cc);

                            cc.IsDeleted = true;
                            cc.IsMultiSelected = false;

                            if (ctlsActive.Contains(cc))
                                ctlsActive.Remove(cc);
                        }

                        //if (parent != null && parent is Canvas && parent != Util.OriginalParent && Util.OriginalParent != Util.OutermostParent)
                        //    (parent as Canvas).Children.Remove(cc);
                    }

                    Util.SelectedControls.Clear();

                    var outerParent = Parent;

                    if (outerParent != null && outerParent is Canvas)
                    {
                        if (this is CFrame && (this as CFrame).Content == Util.OutermostParent)
                            break;

                        (outerParent as Canvas).Children.Remove(this);
                    }

                    IsMultiSelected = false;
                    IsDeleted = true;

                    if (ctlsActive.Contains(this))
                        ctlsActive.Remove(this);

                    LastFocused = null;
                    break;

                case Key.Left:
                    if (!LockX && !xpos.StartsWith("c") && !xpos.StartsWith("r"))
                        xpos = (GetActualX - 1) + "";

                    Util.BatchMove(-1, 0);
                    break;

                case Key.Right:
                    if (!LockX && !xpos.StartsWith("c") && !xpos.StartsWith("r"))
                        xpos = (GetActualX + 1) + "";

                    Util.BatchMove(1, 0);
                    break;

                case Key.Up:
                    if (!LockY && !ypos.StartsWith("c"))
                        ypos = (GetActualY - 1) + "";

                    Util.BatchMove(0, -1);
                    break;

                case Key.Down:
                    if (!LockY && !ypos.StartsWith("c"))
                        ypos = (GetActualY + 1) + "";

                    Util.BatchMove(0, 1);
                    break;
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var pen = new Pen(new SolidColorBrush(boxColor), 1);
            var brush = Brushes.Transparent;

            if (ismultiselected || LastFocused != null && (LastFocused == this || IsFocused))
            {
                pen.Brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                brush = new SolidColorBrush(Color.FromArgb(100, 93, 173, 226));
            }

            var rect = new Rect(RenderSize);
            drawingContext.DrawRectangle(brush, pen, rect);

            if (LastFocused == this)
            {
                var rectG = new RectangleGeometry
                {
                    Rect = new Rect(rect.Width / 2 - 2, rect.Height / 2 - 2, 4, 4),
                    Transform = new RotateTransform(45, rect.Width / 2, rect.Height / 2)
                };

                drawingContext.DrawGeometry(brush, pen, rectG);
            }
        }
    }
}
