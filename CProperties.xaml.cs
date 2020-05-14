using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for CProperties.xaml
    /// </summary>
    public partial class CProperties : UserControl
    {
        private CControl ctl = null;

        public CProperties()
        {
            InitializeComponent();

            Loaded += (o, e) =>
            {
                CControl.CSControlChanged += (obj, ev) =>
                {
                    if (ctl != obj as CControl) return;

                    ControlName.Content = Util.GetControlName(ctl.GetType());
                    SupplyValues();

                    IdentifyValues();
                };

                CControl.LastFocusedChanged += (obj, ev) =>
                {
                    ctl = CControl.LastFocused;

                    #region Image Property Visibility
                    imageURI.Visibility = ctl != null && ctl is CBitmapImagePanel ? Visibility.Visible : Visibility.Collapsed;
                    imageCOLOR.Visibility = ctl != null && ctl.GetType().Name == "CBitmapImagePanel" ? imageURI.Visibility : Visibility.Collapsed;
                    #endregion

                    #region ImagePanel Property Visibility
                    imageSCALE.Visibility = ctl != null && ctl is CImagePanel ? Visibility.Visible : Visibility.Collapsed;
                    imageFILLCOLOR.Visibility = imageSCALE.Visibility;
                    #endregion

                    #region Label/Button Property Visibility
                    labelALIGN.Visibility = ctl != null && (ctl is CLabel || ctl is CButton) ? Visibility.Visible : Visibility.Collapsed;
                    labelFONT.Visibility = labelALIGN.Visibility;
                    labelTEXT.Visibility = labelALIGN.Visibility;
                    #endregion

                    #region Button Property Visibility
                    buttonCOMMAND.Visibility = ctl != null && ctl is CButton ? Visibility.Visible : Visibility.Collapsed;
                    buttonPAINTBG.Visibility = buttonCOMMAND.Visibility;
                    #endregion

                    if (ctl == null)
                    {
                        ClearValues();
                        return;
                    }

                    ControlName.Content = Util.GetControlName(ctl.GetType());
                    SupplyValues();

                    IdentifyValues();
                };

                #region Generic Control Text Handlers
                fieldName.KeyUp += (obj, ev) =>
                {
                    if (ctl == null)
                    {
                        fieldName.Text = string.Empty;
                        return;
                    }

                    if (ev.Key == Key.Enter)
                    {
                        if (fieldName.Text == string.Empty)
                        {
                            fieldName.Text = ctl.fieldname;
                            return;
                        }

                        if (!Util.VerifyNameuniqueness(fieldName.Text, ctl))
                        {
                            MessageBox.Show("Name <" + fieldName.Text + "> is already used by other controls.", "Name Already Taken");
                            fieldName.Text = ctl.fieldname;
                            return;
                        }

                        ctl.fieldname = fieldName.Text;
                    }
                };

                fieldName.LostFocus += (obj, ev) =>
                {
                    if (ctl == null)
                    {
                        fieldName.Text = string.Empty;
                        return;
                    }

                    if (fieldName.Text == string.Empty)
                    {
                        fieldName.Text = ctl.fieldname;
                        return;
                    }

                    if (!Util.VerifyNameuniqueness(fieldName.Text, ctl))
                    {
                        MessageBox.Show("Name <" + fieldName.Text + "> is already used by other controls.", "Name Already Taken");
                        fieldName.Text = ctl.fieldname;
                        return;
                    }

                    ctl.fieldname = fieldName.Text;
                };

                xpos.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.xpos = xpos.Text;
                };

                ypos.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.ypos = ypos.Text;
                };

                Lock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.Locked = Lock.IsLocked;
                    PropertyStack.IsEnabled = !Lock.IsLocked;
                };

                //xpos.KeyUp += (obj, ev) =>
                //{
                //    if (ctl == null) return;

                //    if (ev.Key == Key.Enter)
                //        ctl.xpos = xpos.Text;
                //};

                //xpos.LostKeyboardFocus += (obj, ev) =>
                //{
                //    if (ctl == null) return;
                //    ctl.xpos = xpos.Text;
                //};

                //ypos.KeyUp += (obj, ev) =>
                //{
                //    if (ctl == null) return;

                //    if (ev.Key == Key.Enter)
                //        ctl.ypos = ypos.Text;
                //};

                //ypos.LostKeyboardFocus += (obj, ev) =>
                //{
                //    if (ctl == null) return;
                //    ctl.ypos = ypos.Text;
                //};

                wide.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.wide = wide.Text;

                    //int w = Util.StrToInt(ctl, wide.Text);

                    //if (w != Util.ErrorInt)
                    //    ctl.Width = w;
                };

                tall.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.tall = tall.Text;

                    //int t = Util.StrToInt(ctl, tall.Text);

                    //if (t != Util.ErrorInt)
                    //    ctl.Height = t;
                };

                enabled.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.enabled = enabled.Text;
                };

                visible.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.visible = visible.Text;
                };
                #endregion

                #region BitmapImagePanel Handlers
                image.KeyUp += (obj, ev) =>
                {
                    if (ctl == null) return;
                    if (ev.Key == Key.Enter)
                        (ctl as CBitmapImagePanel).image = image.Text.Trim();
                };

                image.LostFocus += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CBitmapImagePanel).image = image.Text.Trim();
                };

                imagecolor.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CBitmapImagePanel).imagecolor = imagecolor.Text;
                };
                #endregion

                #region ImagePanel Handlers
                scaleImage.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CImagePanel).scaleimage = scaleImage.Text;
                };

                fillcolor.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CImagePanel).fillcolor = fillcolor.Text;
                };
                #endregion

                #region Label/Button Handlers
                labelText.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (ctl is CLabel)
                        (ctl as CLabel).labeltext = labelText.Text;
                    else if (ctl is CButton)
                        (ctl as CButton).labeltext = labelText.Text;
                };

                textAlignment.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (ctl is CLabel)
                        (ctl as CLabel).textalignment = textAlignment.Text;
                    else if (ctl is CButton)
                        (ctl as CButton).textalignment = textAlignment.Text;
                };

                font.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (ctl is CLabel)
                        (ctl as CLabel).font = font.Text;
                    else if (ctl is CButton)
                        (ctl as CButton).font = font.Text;
                };
                #endregion

                #region Button Handlers
                command.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CButton).command = command.Text;
                };

                paintbackground.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CButton).paintbackground = paintbackground.Text;
                };
                #endregion
            };
        }

        private void IdentifyValues()
        {
            switch (ctl.GetType().Name)
            {
                case "CBitmapImagePanel":
                    var cbmp = ctl as CBitmapImagePanel;

                    image.Text = cbmp.image;
                    imagecolor.Text = cbmp.imagecolor;
                    break;

                case "CImagePanel":
                    var cimg = ctl as CImagePanel;

                    image.Text = cimg.image;
                    scaleImage.Text = cimg.scaleimage + "";
                    break;

                case "CLabel":
                    var clbl = ctl as CLabel;

                    labelText.Text = clbl.labeltext;
                    textAlignment.Text = clbl.textalignment;
                    font.Text = clbl.font;
                    break;

                case "CButton":
                case "CMouseOverPanelButton":
                    var cbtn = ctl as CButton;

                    labelText.Text = cbtn.labeltext;
                    textAlignment.Text = cbtn.textalignment;
                    command.Text = cbtn.command;
                    paintbackground.Text = cbtn.paintbackground;
                    font.Text = cbtn.font;
                    break;
            }
        }

        #region Local Usable Methods
        private void SupplyValues()
        {
            Lock.Visibility = Visibility.Visible;
            PropertyStack.IsEnabled = !ctl.Locked;

            ControlName.Content = Util.GetControlName(ctl.GetType());
            Lock.IsLocked = ctl.Locked;
            fieldName.Text = ctl.fieldname;
            xpos.Text = ctl.xpos;
            ypos.Text = ctl.ypos;
            wide.Text = ctl.wide;
            tall.Text = ctl.tall;
            enabled.Text = ctl.enabled;
            visible.Text = ctl.visible;
        }

        private void ClearValues()
        {
            Lock.Visibility = Visibility.Hidden;

            ControlName.Content = "No Control Selected.";
            fieldName.Text = "";
            xpos.Text = "";
            ypos.Text = "";
            wide.Text = "";
            tall.Text = "";
            enabled.Text = "";
            visible.Text = "";
        }
        #endregion
    }
}
