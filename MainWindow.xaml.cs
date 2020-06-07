using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filename = string.Empty;
        private string safeFilename = string.Empty;
        private Process proc = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Loaded += (o, e) =>
                {
                    if (!File.Exists(Directory.GetCurrentDirectory() + "\\TgaLib.dll"))
                    {
                        MessageBox.Show("TgaLib.dll is missing. Please reinstall the program or obtain a copy on the internet.", "DLL File Missing");

                        Close();
                        return;
                    }

                // Get reference to canvas element for drawing elements.
                Util.OutermostParent = ControlArea;
                    Util.OriginalParent = ControlArea;

                    OpenFile();

                //Toolbox.CControlArea = ControlArea;

                ControlArea.MouseMove += (obj, ev) =>
                {
                    var ctl = CControl.LastFocused;

                    if (ctl == null || !ctl.IsEnabled || ctl.Locked || !Util.IsCtrlUp)
                        return;

                    #region Object Movement
                    if (ev.LeftButton == MouseButtonState.Pressed)
                    {
                        Mouse.OverrideCursor = Cursors.Cross;

                        var outParent = ev.GetPosition(ctl.Parent as UIElement);
                        var radiusBias = (ctl.ActualWidth + ctl.ActualHeight) * 0.25;

                        if (outParent.X < ctl.GetActualX - radiusBias || outParent.X > ctl.GetActualX + ctl.ActualWidth + radiusBias)
                            return;

                        if (outParent.Y < ctl.GetActualY - radiusBias || outParent.Y > ctl.GetActualY + ctl.ActualHeight + radiusBias)
                            return;

                        var newPoint = ev.GetPosition(ctl.Parent as Canvas);

                        var oldX = ctl.GetActualX;
                        var oldY = ctl.GetActualY;

                        var xDiff = (int)(newPoint.X - ctl.LastPoint.X);
                        var yDiff = (int)(newPoint.Y - ctl.LastPoint.Y);

                        // lock the control if fixated to the center or to the right
                        if (!ctl.LockX && !ctl.xpos.StartsWith("c") && !ctl.xpos.StartsWith("r"))
                                ctl.xpos = xDiff + "";
                        //Canvas.SetLeft(ctl, (int)(newPoint.X - ctl.LastPoint.X));

                        if (!ctl.LockY && !ctl.ypos.StartsWith("c"))
                                ctl.ypos = yDiff + "";
                        //Canvas.SetTop(ctl, (int)(newPoint.Y - ctl.LastPoint.Y));

                        if (ctl is CFrame)
                                if ((ctl as CFrame).Content == Util.OutermostParent)
                                    return;

                        oldX -= ctl.GetActualX;
                        oldY -= ctl.GetActualY;

                        foreach (var cc in Util.SelectedControls.Where(p => p != ctl))
                        {
                            if (cc.Locked || cc.enabled != "1")
                                continue;

                            var xplower = cc.xpos.ToLower().Trim();
                            var yplower = cc.ypos.ToLower().Trim();

                            if (!cc.LockX && !(xplower.StartsWith("c") || xplower.StartsWith("r")))
                                cc.xpos = (cc.GetActualX - oldX) + "";

                            if (!cc.LockY && !yplower.StartsWith("c"))
                                cc.ypos = (cc.GetActualY - oldY) + "";
                        }
                    }
                    #endregion

                    #region Object Resizing
                    else if (ev.RightButton == MouseButtonState.Pressed)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;

                        var newPoint = ev.GetPosition(ctl);

                        //var oldW = ctl.ActualWidth;
                        //var oldH = ctl.ActualHeight;

                        var nw = newPoint.X < ctl.MinWidth ? ctl.MinWidth : newPoint.X;
                        var nh = newPoint.Y < ctl.MinHeight ? ctl.MinHeight : newPoint.Y;

                        if (!ctl.LockWidth)
                            ctl.wide = nw + "";

                        if (!ctl.LockHeight)
                            ctl.tall = nh + "";

                        #region TODO: Make a better batch resizing solution

                        //if (nw > 0)
                        //    oldW /= nw;
                        //else
                        //    oldW = 0.0;

                        //if (nh > 0)
                        //    oldH /= nh;
                        //else
                        //    oldH = 0.0;

                        //foreach (var cc in Util.SelectedControls.Where(p => p != ctl))
                        //{
                        //    var ccxW = (cc.ActualWidth - cc.ActualWidth * oldW) + cc.ActualWidth;
                        //    var ccxH = (cc.ActualHeight - cc.ActualHeight * oldH) + cc.ActualHeight;

                        //    cc.wide = (int)ccxW + "";
                        //    cc.tall = (int)ccxH + "";
                        //}

                        //ctl.Width = (int)nw;
                        //ctl.Height = (int)nh;
                        #endregion
                    }
                    #endregion

                    else
                        Mouse.OverrideCursor = Cursors.Arrow;
                };
                
                #region Menu Item Handlers
                StripOpen.Click += (obj, ev) => OpenFile();
                    StripSave.Click += (obj, ev) => Util.SaveFile();

                    StripDelete.Click += (obj, ev) =>
                    {
                        foreach (var cc in Util.SelectedControls.Where(p => p != CControl.LastFocused))
                        {
                            if (cc.Locked)
                                continue;

                            ((cc as CControl).Parent as Canvas).Children.Remove(cc);
                            cc.IsDeleted = true;
                        }
                        Util.SelectedControls.Clear();

                        if (CControl.LastFocused == null)
                            return;

                        if (!CControl.LastFocused.Locked)
                        {
                            Util.OutermostParent.Children.Remove(CControl.LastFocused);
                            CControl.LastFocused.IsDeleted = true;
                            CControl.LastFocused = null;
                        }
                    };

                    CControl.CSControlChanged += (obj, ev) =>
                    {
                        StripBringToFront.IsEnabled =
                        StripSendToBack.IsEnabled =
                        StripClearSelect.IsEnabled =
                        StripDelete.IsEnabled = !(CControl.LastFocused == null || CControl.LastFocused.Locked);
                    };

                    StripBringToFront.Click += (obj, ev) => Util.ArrangeControl(CControl.LastFocused);
                    StripSendToBack.Click += (obj, ev) => Util.ArrangeControl(CControl.LastFocused, false);

                    StripClearSelect.Click += (obj, ev) =>
                    {
                        if (CControl.LastFocused == null)
                            return;

                    //var tempCtl = CControl.LastFocused;
                    CControl.LastFocused = null;
                    //tempCtl.InvalidateVisual();

                    foreach (var cc in Util.SelectedControls)
                            cc.IsMultiSelected = false;
                        Util.SelectedControls.Clear();
                    };

                    StripReset.Click += (obj, ev) => Load();
                    StripAbout.Click += (obj, ev) => new About().Show();

                    TGALoader.Click += (obj, ev) =>
                    {
                        if (!File.Exists(Directory.GetCurrentDirectory() + "\\PNGToTGA.exe"))
                        {
                            MessageBox.Show("PNGToTGA.exe is missing. Cannot launch the application.", "Application Not Found");
                            TGALoader.IsEnabled = false;
                            return;
                        }

                        if (proc == null || proc.HasExited)
                            proc = Process.Start(new ProcessStartInfo("PNGToTGA.exe"));
                    };
                #endregion

                PreviewKeyDown += (obj, ev) =>
                    {
                        if (Util.IsCtrlUp && ev.Key == Key.LeftCtrl || ev.Key == Key.RightCtrl)
                            Util.IsCtrlUp = false;

                        if (ev.Key == Key.OemOpenBrackets)
                            Util.ArrangeSelectedControls();
                        else if (ev.Key == Key.OemCloseBrackets)
                            Util.ArrangeSelectedControls(false);
                    };
                PreviewKeyUp += (obj, ev) =>
                {
                    if (!Util.IsCtrlUp)
                        Util.IsCtrlUp = true;
                };

                PreviewMouseUp += (obj, ev) =>
                {
                    if (ev.ChangedButton == MouseButton.Left)
                    {
                        if (Util.IsCtrlUp && !(PropertiesPane.IsFocused || PropertiesPane.IsKeyboardFocusWithin))
                            Util.ClearSelection();
                    }
                };

                var keyMgr = new KeyManager(this);
                keyMgr.KeyProcessed += (obj, ev) =>
                {
                    switch (ev.KeyCombinations)
                    {
                        case "LeftCtrl+S":
                        case "RightCtrl+S":
                        // TODO: Define a saving mechanism here
                        Util.SaveFile();
                            break;

                        case "LeftCtrl+O":
                        case "RightCtrl+O":
                            OpenFile();
                            break;

                        case "LeftCtrl+R":
                        case "RightCtrl+R":
                            Load();
                            break;

                    //case "OemOpenBrackets":
                    //    //Util.ArrangeControl(CControl.LastFocused);

                    //    break;

                    //case "Oem6":
                    //    //Util.ArrangeControl(CControl.LastFocused, false);

                    //    break;

                    case "LeftCtrl+C":
                        case "RightCtrl+C":
                        //_ = CControl.LastFocused.CopyBasicValues;
                        Util.CopyControl(CControl.LastFocused);
                            break;

                        case "LeftCtrl+V":
                        case "RightCtrl+V":
                            Util.PasteControl();
                            Util.PasteControls();
                            break;
                    }
                };
            };
            } catch (Exception e)
            {
                MessageBox.Show("An error occured. Reason: " + e.Message, "Runtime Error");
            }
        }

        private void Load()
        {
            if (filename == string.Empty)
                return;

            if (!File.Exists(filename))
            {
                MessageBox.Show("File not found: " + filename, "Load Failed");
                Util.OriginalFileName = string.Empty;
                return;
            }

            Mouse.PrimaryDevice.OverrideCursor = Cursors.Wait;

            ControlArea.Children.Clear();

            if (Util.OutermostParent != null)
                Util.OutermostParent.Children.Clear();
            Util.OutermostParent = null;

            foreach (var cc in Util.SelectedControls)
                cc.IsDeleted = true;

            Util.SelectedControls.Clear();
            Util.OriginalFileName = string.Empty;
            Util.CopiedControl = null;
            CControl.LastFocused = null;
            CControl.ClearActiveControls();

            var ccontrols = Util.ParseFile(filename);
            var parented = false;

            if (ccontrols.Length == 0)
            {
                MessageBox.Show("Cannot parse file: " + filename, "Parse Failed");
                Mouse.PrimaryDevice.OverrideCursor = Cursors.Arrow;
                Title = "Counter Strike 1.6 Resource UI Themer";

                return;
            }

            Title = safeFilename + " - Counter Strike 1.6 Resource UI Themer";
            
            //.ClearActiveControls();

            //GC.Collect();

            Util.OriginalFileName = filename;
            CControl bgControl = null;

            #region Cheap hack for centering element
            int bgX = -1;
            int bgY = -1;

            switch (safeFilename)
            {
                case "BuyEquipment_CT.res":
                case "BuyEquipment_TER.res":
                case "BuyMachineguns_CT.res":
                case "BuyMachineguns_TER.res":
                case "BuyMenu.res":
                case "BuyPistols_CT.res":
                case "BuyPistols_TER.res":
                case "BuyRifles_CT.res":
                case "BuyRifles_TER.res":
                case "BuyShotguns_CT.res":
                case "BuyShotguns_TER.res":
                case "BuySubMachineguns_CT.res":
                case "BuySubMachineguns_TER.res":
                case "MainBuyMenu.res":
                    break;

                case "BuyEquipment.res":
                case "MOTD.res":
                case "Classmenu.res":
                case "Classmenu_CT.res":
                case "Classmenu_TER.res":
                case "Teammenu.res":
                    bgX = 0;
                    bgY = 0;
                    break;
            }
            #endregion

            switch (safeFilename)
            {
                case "BuyEquipment.res":
                case "BuyEquipment_CT.res":
                case "BuyEquipment_TER.res":
                case "BuyMachineguns_CT.res":
                case "BuyMachineguns_TER.res":
                case "BuyMenu.res":
                case "BuyPistols_CT.res":
                case "BuyPistols_TER.res":
                case "BuyRifles_CT.res":
                case "BuyRifles_TER.res":
                case "BuyShotguns_CT.res":
                case "BuyShotguns_TER.res":
                case "BuySubMachineguns_CT.res":
                case "BuySubMachineguns_TER.res":
                case "Classmenu_CT.res":
                case "Classmenu_TER.res":
                case "Teammenu.res":
                case "MainBuyMenu.res":
                case "MOTD.res":
                    var bgpanelPath = Directory.GetParent(filename) + "\\" + "BackgroundPanel.res";

                    if (File.Exists(bgpanelPath))
                    {
                        var bgcontrols = Util.ParseFile(bgpanelPath, false);

                        if (bgcontrols.Length == 0)
                        {
                            MessageBox.Show("BackgroundPanel.res doesn't have any controls.", "BackgroundPanel.res Empty");
                            break;
                        }

                        var bgRoot = bgcontrols[0] as CFrame;
                        bgControl = bgRoot;

                        foreach (CControl ccontrol in bgcontrols.Where(p => p != bgRoot))
                        {
                            bgRoot.Content.Children.Add(ccontrol);
                            ccontrol.IsEnabled = false;
                            ccontrol.Discard = true;
                        }

                        ControlArea.Children.Add(bgRoot);
                        Util.ForceFrameUpdate(bgRoot);
                        Panel.SetZIndex(bgRoot.Content, -32767);

                        bgRoot.IsEnabled = false;
                        bgRoot.Discard = true;
                    }

                    break;
            }

            #region Identify Outmost Parent
            switch (ccontrols[0].controlname)
            {
                case "Frame":
                case "WizardPanel":
                case "WizardSubPanel":
                case "CClientScoreBoardDialog":
                case "CTeamMenu":
                    Util.OutermostParent = (ccontrols[0] as CFrame).Content;
                    parented = true;
                    break;
            }

            var pcontrol = ccontrols[0];

            if (parented)
            {
                foreach (CControl ccontrol in ccontrols.Where(p => p != ccontrols[0]))
                {
                    Util.OutermostParent.Children.Add(ccontrol);
                    Util.ForceFrameUpdate(ccontrol);

                    ccontrol.OldParent = (pcontrol as CFrame).Content;
                }

                ControlArea.Children.Add(pcontrol);
                Util.ForceFrameUpdate(pcontrol);

                if (bgControl != null)
                {
                    bgControl.xpos = bgX != -1 ? bgX + "" : ccontrols[0].xpos;
                    bgControl.ypos = bgY != -1 ? bgY + "" : ccontrols[0].ypos;
                }
            }
            else
            {
                foreach (CControl ccontrol in ccontrols)
                {
                    ControlArea.Children.Add(ccontrol);
                    ccontrol.OldParent = ControlArea;
                }
            }

            Util.ContainerControl = pcontrol;

            Util.SelectedControls.Clear();
            CControl.LastFocused = null;

            //Panel.SetZIndex(Util.OutermostParent, -999);
            #endregion

            Mouse.PrimaryDevice.OverrideCursor = Cursors.Arrow;
        }

        private void OpenFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Resource UI File|*.res",
                DefaultExt = ".res",
                Title = "Select .res file to edit (must be inside cstrike or czero directory)",
                Multiselect = false
            };

            ofd.FileOk += (obj, ev) =>
            {
                filename = ofd.FileName;
                safeFilename = ofd.SafeFileName;

                Load();
            };

            ofd.ShowDialog(this);
        }
    }
}
