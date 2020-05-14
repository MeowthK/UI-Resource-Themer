using Microsoft.Win32;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (o, e) =>
            {
                // Get reference to canvas element for drawing elements.
                Util.OutermostParent = ControlArea;
                Util.OriginalParent = ControlArea;

                OpenFile();

                //Toolbox.CControlArea = ControlArea;

                ControlArea.MouseMove += (obj, ev) =>
                {
                    var ctl = CControl.LastFocused;

                    if (ctl == null || !ctl.IsEnabled || ctl.Locked)
                        return;

                    #region Object Movement
                    if (ev.LeftButton == MouseButtonState.Pressed)
                    {
                        var newPoint = ev.GetPosition(ctl.Parent as Canvas);

                        // lock the control if fixated to the center or to the right
                        if (!ctl.xpos.StartsWith("c") && !ctl.xpos.StartsWith("r"))
                            ctl.xpos = ((int)newPoint.X - ctl.LastPoint.X) + "";
                        //Canvas.SetLeft(ctl, (int)(newPoint.X - ctl.LastPoint.X));

                        if (!ctl.ypos.StartsWith("c"))
                            ctl.ypos = ((int)newPoint.Y - ctl.LastPoint.Y) + "";
                        //Canvas.SetTop(ctl, (int)(newPoint.Y - ctl.LastPoint.Y));
                    }
                    #endregion

                    #region Object Resizing
                    else if (ev.RightButton == MouseButtonState.Pressed)
                    {
                        var newPoint = ev.GetPosition(ctl);

                        var nw = newPoint.X < ctl.MinWidth ? ctl.MinWidth : newPoint.X;
                        var nh = newPoint.Y < ctl.MinHeight ? ctl.MinHeight : newPoint.Y;

                        ctl.wide = nw + "";
                        ctl.tall = nh + "";

                        //ctl.Width = (int)nw;
                        //ctl.Height = (int)nh;
                    }
                    #endregion
                };

                #region Menu Item Handlers
                StripOpen.Click += (obj, ev) => OpenFile();
                StripSave.Click += (obj, ev) => Util.SaveFile();

                StripDelete.Click += (obj, ev) =>
                {
                    if (CControl.LastFocused == null)
                        return;

                    if (!CControl.LastFocused.Locked)
                        Util.OutermostParent.Children.Remove(CControl.LastFocused);
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

                    var tempCtl = CControl.LastFocused;
                    CControl.LastFocused = null;
                    tempCtl.InvalidateVisual();
                };

                StripReset.Click += (obj, ev) => Load();
                StripAbout.Click += (obj, ev) => new About().Show();
                #endregion

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

                        case "OemOpenBrackets":
                            Util.ArrangeControl(CControl.LastFocused);
                            break;

                        case "Oem6":
                            Util.ArrangeControl(CControl.LastFocused, false);
                            break;
                    }
                };
            };
        }

        private void Load()
        {
            if (filename == string.Empty)
                return;

            var ccontrols = Util.ParseFile(filename);
            var parented = false;

            if (ccontrols.Length == 0)
            {
                MessageBox.Show("Cannot parse file: " + filename, "Parse Failed");
                return;
            }

            Title = safeFilename + " - Counter Strike 1.6 Resource UI Themer";

            ControlArea.Children.Clear();
            Util.OutermostParent.Children.Clear();

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
                        var bgcontrols = Util.ParseFile(bgpanelPath);

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
                        Panel.SetZIndex(bgRoot.Content, -999);

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
            #endregion
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
