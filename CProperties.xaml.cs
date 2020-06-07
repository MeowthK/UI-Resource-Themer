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
                header.KeyUp += (obj, ev) =>
                {
                    if (ctl == null)
                    {
                        header.Text = string.Empty;
                        return;
                    }

                    if (ev.Key == Key.Enter)
                    {
                        //if (header.Text == string.Empty)
                        //{
                        //    header.Text = ctl.headerName;
                        //    return;
                        //}

                        if (!Util.VerifyHeaderUniqueness(header.Text, ctl))
                        {
                            MessageBox.Show("Header <" + header.Text + "> is already assigned to other controls.", "Header Duplicate");
                            header.Text = ctl.headerName;
                            CControl.LastFocused = null;
                            return;
                        }

                        ctl.headerName = header.Text;
                    }
                };

                header.LostFocus += (obj, ev) =>
                {
                    if (ctl == null)
                    {
                        header.Text = string.Empty;
                        return;
                    }

                    //if (header.Text == string.Empty)
                    //{
                    //    header.Text = ctl.headerName;
                    //    return;
                    //}

                    if (!Util.VerifyHeaderUniqueness(header.Text, ctl))
                    {
                        MessageBox.Show("Header <" + header.Text + "> is already assigned to other controls.", "Header Duplicate");
                        header.Text = ctl.headerName;
                        CControl.LastFocused = null;
                        return;
                    }

                    ctl.headerName = header.Text;
                };

                //header.TextChanged += (obj, ev) =>
                //{
                //    if (ctl == null)
                //    {
                //        header.Text = string.Empty;
                //        return;
                //    }

                //    ctl.headerName = header.Text;
                //};

                fieldName.TextChanged += (obj, ev) =>
                {
                    if (ctl == null)
                    {
                        fieldName.Text = string.Empty;
                        return;
                    }

                    ctl.fieldname = fieldName.Text;
                };

                //fieldName.KeyUp += (obj, ev) =>
                //{
                //    if (ctl == null)
                //    {
                //        fieldName.Text = string.Empty;
                //        return;
                //    }

                //    if (ev.Key == Key.Enter)
                //    {
                //        if (fieldName.Text == string.Empty)
                //        {
                //            fieldName.Text = ctl.fieldname;
                //            return;
                //        }

                //        //if (!Util.VerifyNameuniqueness(fieldName.Text, ctl))
                //        //{
                //        //    MessageBox.Show("Name <" + fieldName.Text + "> is already used by other controls.", "Name Already Taken");
                //        //    fieldName.Text = ctl.fieldname;
                //        //    CControl.LastFocused = null;
                //        //    return;
                //        //}

                //        ctl.fieldname = fieldName.Text;
                //    }
                //};

                //fieldName.LostFocus += (obj, ev) =>
                //{
                //    if (ctl == null)
                //    {
                //        fieldName.Text = string.Empty;
                //        return;
                //    }

                //    if (fieldName.Text == string.Empty)
                //    {
                //        fieldName.Text = ctl.fieldname;
                //        return;
                //    }

                //    //if (!Util.VerifyNameuniqueness(fieldName.Text, ctl))
                //    //{
                //    //    MessageBox.Show("Name <" + fieldName.Text + "> is already used by other controls.", "Name Already Taken");
                //    //    fieldName.Text = ctl.fieldname;
                //    //    CControl.LastFocused = null;
                //    //    return;
                //    //}

                //    ctl.fieldname = fieldName.Text;
                //};

                xpos.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (!ctl.LockX)
                        ctl.xpos = xpos.Text;

                    if (xpos.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked && !cc.LockX && cc.IsEnabled)
                                cc.xpos = xpos.Text;
                        }
                    }
                };

                ypos.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (!ctl.LockY)
                        ctl.ypos = ypos.Text;

                    if (ypos.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked && !cc.LockY && cc.IsEnabled)
                                cc.ypos = ypos.Text;
                        }
                    }
                };

                Lock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.Locked = Lock.IsLocked;

                    if (LockToggler.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                            cc.Locked = Lock.IsLocked;
                    }

                    PropertyStack.IsEnabled = !Lock.IsLocked;
                };

                xposLock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.LockX = xposLock.IsLocked;

                    xpos.IsEnabled = !xposLock.IsLocked;
                };

                yposLock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.LockY = yposLock.IsLocked;

                    ypos.IsEnabled = !yposLock.IsLocked;
                };

                wideLock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.LockWidth = wideLock.IsLocked;

                    wide.IsEnabled = !wideLock.IsLocked;
                };

                tallLock.LockChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.LockHeight = tallLock.IsLocked;

                    tall.IsEnabled = !tallLock.IsLocked;
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

                    if (!ctl.LockWidth)
                        ctl.wide = wide.Text;

                    if (wide.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked && !cc.LockWidth && cc.IsEnabled)
                                cc.wide = wide.Text;
                        }
                    }

                    //int w = Util.StrToInt(ctl, wide.Text);

                    //if (w != Util.ErrorInt)
                    //    ctl.Width = w;
                };

                tall.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (!ctl.LockHeight)
                        ctl.tall = tall.Text;

                    if (tall.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked && !cc.LockHeight && cc.IsEnabled)
                                cc.tall = tall.Text;
                        }
                    }

                    //int t = Util.StrToInt(ctl, tall.Text);

                    //if (t != Util.ErrorInt)
                    //    ctl.Height = t;
                };

                enabled.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.enabled = enabled.Text;

                    if (enabled.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked)
                                cc.enabled = enabled.Text;
                        }
                    }
                };

                visible.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    ctl.visible = visible.Text;

                    if (visible.IsFocused)
                    {
                        foreach (var cc in Util.SelectedControls)
                        {
                            if (!cc.Locked)
                                cc.visible = visible.Text;
                        }
                    }
                };
                #endregion

                #region BitmapImagePanel Handlers
                image.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CBitmapImagePanel).image = image.Text.Trim();

                    if (!image.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CBitmapImagePanel)
                            (cc as CBitmapImagePanel).image = image.Text.Trim();
                    }
                };

                //image.KeyUp += (obj, ev) =>
                //{
                //    if (ctl == null) return;
                //    if (ev.Key == Key.Enter)
                //    {
                //        (ctl as CBitmapImagePanel).image = image.Text.Trim();

                //        foreach (var cc in Util.SelectedControls)
                //        {
                //            if (!cc.Locked && cc is CBitmapImagePanel)
                //                (cc as CBitmapImagePanel).image = image.Text.Trim();
                //        }
                //    }
                //};

                //image.LostFocus += (obj, ev) =>
                //{
                //    if (ctl == null) return;
                //    (ctl as CBitmapImagePanel).image = image.Text.Trim();

                //    foreach (var cc in Util.SelectedControls)
                //    {
                //        if (!cc.Locked && cc is CBitmapImagePanel)
                //            (cc as CBitmapImagePanel).image = image.Text.Trim();
                //    }
                //};

                imagecolor.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CBitmapImagePanel).imagecolor = imagecolor.Text;

                    if (!imagecolor.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CBitmapImagePanel)
                            (cc as CBitmapImagePanel).imagecolor = imagecolor.Text;
                    }
                };
                #endregion

                #region ImagePanel Handlers
                scaleImage.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CImagePanel).scaleimage = scaleImage.Text;

                    if (!scaleImage.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CImagePanel)
                            (cc as CImagePanel).scaleimage = scaleImage.Text;
                    }
                };

                fillcolor.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CImagePanel).fillcolor = fillcolor.Text;

                    if (!fillcolor.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CImagePanel)
                            (cc as CImagePanel).fillcolor = fillcolor.Text;
                    }
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

                    if (!labelText.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (cc.Locked)
                            continue;

                        if (cc is CLabel)
                            (cc as CLabel).labeltext = labelText.Text;
                        else if (cc is CButton)
                            (cc as CButton).labeltext = labelText.Text;
                    }
                };

                textAlignment.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (ctl is CLabel)
                        (ctl as CLabel).textalignment = textAlignment.Text;
                    else if (ctl is CButton)
                        (ctl as CButton).textalignment = textAlignment.Text;

                    if (!textAlignment.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (cc.Locked)
                            continue;

                        if (cc is CLabel)
                            (cc as CLabel).textalignment = textAlignment.Text;
                        else if (cc is CButton)
                            (cc as CButton).textalignment = textAlignment.Text;
                    }
                };

                font.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;

                    if (ctl is CLabel)
                        (ctl as CLabel).font = font.Text;
                    else if (ctl is CButton)
                        (ctl as CButton).font = font.Text;

                    if (!font.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (cc.Locked)
                            continue;

                        if (cc is CLabel)
                            (cc as CLabel).font = font.Text;
                        else if (cc is CButton)
                            (cc as CButton).font = font.Text;
                    }
                };
                #endregion

                #region Button Handlers
                command.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CButton).command = command.Text;

                    if (!command.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CButton)
                            (cc as CButton).command = command.Text;
                    }
                };

                paintbackground.TextChanged += (obj, ev) =>
                {
                    if (ctl == null) return;
                    (ctl as CButton).paintbackground = paintbackground.Text;

                    if (!paintbackground.IsFocused)
                        return;

                    foreach (var cc in Util.SelectedControls)
                    {
                        if (!cc.Locked && cc is CButton)
                            (cc as CButton).paintbackground = paintbackground.Text;
                    }
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
            xposLock.Visibility = Visibility.Visible;
            yposLock.Visibility = Visibility.Visible;
            wideLock.Visibility = Visibility.Visible;
            tallLock.Visibility = Visibility.Visible;

            PropertyStack.IsEnabled = !ctl.Locked;
            xpos.IsEnabled = !ctl.LockX;
            ypos.IsEnabled = !ctl.LockY;
            wide.IsEnabled = !ctl.LockWidth;
            tall.IsEnabled = !ctl.LockHeight;

            Lock.IsLocked = ctl.Locked;
            xposLock.IsLocked = ctl.LockX;
            yposLock.IsLocked = ctl.LockY;
            wideLock.IsLocked = ctl.LockWidth;
            tallLock.IsLocked = ctl.LockHeight;

            ControlName.Content = Util.GetControlName(ctl.GetType());
            
            header.Text = ctl.headerName;
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
            xposLock.Visibility = Visibility.Hidden;
            yposLock.Visibility = Visibility.Hidden;
            wideLock.Visibility = Visibility.Hidden;
            tallLock.Visibility = Visibility.Hidden;

            ControlName.Content = "No Control Selected.";
            header.Text = "";
            fieldName.Text = "";
            xpos.Text = "";
            ypos.Text = "";
            wide.Text = "";
            tall.Text = "";
            enabled.Text = "";
            visible.Text = "";
        }
        #endregion

        private void LockToggler_Click(object sender, RoutedEventArgs e)
        {
            Lock.IsLocked = !Lock.IsLocked;
        }
    }
}
