using System.Windows;
using System.Windows.Controls;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for CToolbox.xaml
    /// </summary>
    public partial class CToolbox : UserControl
    {
        public Canvas CControlArea { get; set; }

        public CToolbox()
        {
            InitializeComponent();

            Focusable = false;
            IsTabStop = false;

            Loaded += (o, e) =>
            {
                AddPanel.Click += (obj, ev) => AddToChildren(new CPanel());
                AddBitmapImagePanel.Click += (obj, ev) => AddToChildren(new CBitmapImagePanel());
                AddImagePanel.Click += (obj, ev) => AddToChildren(new CImagePanel());
                AddLabel.Click += (obj, ev) => AddToChildren(new CLabel());
                AddButton.Click += (oj, ev) => AddToChildren(new CButton());
                AddMOPButton.Click += (oj, ev) => AddToChildren(new CMouseOverPanelButton());
            };
        }

        private void AddToChildren(CControl control)
        {
            if (Util.OutermostParent == null)
            {
                Util.OutermostParent = Util.OriginalParent;
                //MessageBox.Show("Control Area's reference is not set. Cannot initialize toolbox.", "ControlArea Uninitialized");
                //return;
            }

            Canvas parent = Util.OutermostParent;
            //MessageBox.Show(parent.Name);

            //if (CControl.LastFocused == null)
            //    parent = CControlArea;
            //if (CControl.LastFocused is CFrame)
            //    parent = (CControl.LastFocused as CFrame).Content;

            control.fieldname = Util.GetFactoryName(control, parent);
            control.wide = "64";
            control.tall = "24";

            parent.Children.Add(control);
            control.OldParent = parent;

            Panel.SetZIndex(control, Util.MaxZ);

            control.xpos = (int)(parent.Width / 2 - control.ActualWidth / 2) + "";
            control.ypos = (int)(parent.Height / 2 - control.ActualHeight / 2) + "";

            //Canvas.SetLeft(control, parent.Width / 2 - control.Width / 2);
            //Canvas.SetTop(control, parent.Height / 2 - control.Height / 2);

            if (control.GetActualX < -32767)
                control.xpos = "0";
            if (control.GetActualY < -32767)
                control.ypos = "0";

            control.Focus();
        }
    }
}
