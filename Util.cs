using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public enum Comparison
{
    EQUAL, GREATER, LESSER, UNKNOWN
};

namespace UI_Resource_Themer
{
    public static class Util
    {
        public static readonly Brush DefaultBG = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        public static readonly Brush DefaultFG = new SolidColorBrush(Color.FromArgb(255, 255, 174, 0));
        public static readonly Pen EmptyPen = new Pen(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), 0);
        public static readonly HashSet<CControl> SelectedControls = new HashSet<CControl>();
        public static bool IsCtrlUp = true;

        public static string HomeDir { get; set; }
        public static string WorkDir { get; set; }
        public static string OriginalFileName { get; set; } = string.Empty;
        public static CControl ContainerControl { get; set; } = null;
        public static LoadingDialog LoadDialog { get; set; }

        public static CControl CopiedControl { get; set; }
        public static readonly Color EmptyColor = Color.FromArgb(0, 0, 0, 0);
        public static readonly double EmSize = 10.0;
        public static readonly double LabelMargin = 6.0;
        public static Canvas OutermostParent { get; set; }
        public static Canvas OriginalParent { get; set; }

        public static int ErrorInt = -32767;

        public static int MaxZ { get; private set; } = 0;

        private static string topMostHeader = string.Empty;
        private static readonly Dictionary<string, Type> controlTypes = new Dictionary<string, Type>
        {
            { "Frame",                   typeof(CFrame)                  },
            { "Panel",                   typeof(CPanel)                  },
            { "WizardPanel",             typeof(CWizardPanel)            },
            { "WizardSubPanel",          typeof(CWizardSubPanel)         },
            { "BitmapImagePanel",        typeof(CBitmapImagePanel)       },
            { "ImagePanel",              typeof(CImagePanel)             },
            { "Label",                   typeof(CLabel)                  },
            { "Button",                  typeof(CButton)                 },
            { "MouseOverPanelButton",    typeof(CMouseOverPanelButton)   },
            { "Divider",                 typeof(CDivider)                },
            { "CClientScoreBoardDialog", typeof(CClientScoreBoardDialog) },
            { "CTeamMenu",               typeof(CTeamMenu)               }
        };

        static Util()
        {
            HomeDir = Directory.GetCurrentDirectory() + @"\cstrike";
            WorkDir = Directory.GetCurrentDirectory() + @"\ThemerTemp";

            //TitleFont.FontFamily = new FontFamily("Verdana Bold");
            //TitleFont.FontWeight = FontWeight.FromOpenTypeWeight(500);
            //TitleFont.FontSize = 14.0;
        }

        public static void ArrangeSelectedControls(bool ToFront = true)
        {
            foreach (var cc in SelectedControls)
                ArrangeControl(cc, ToFront);

            if (CControl.LastFocused != null)
                ArrangeControl(CControl.LastFocused, ToFront);
        }

        public static void ArrangeControl(CControl control, bool ToFront = true, bool Extreme = false)
        {
            if (control == null || !(control.Parent is Canvas))
                return;

            if (control is CFrame && (control as CFrame).Content == OutermostParent)
                return;

            var zidx = Panel.GetZIndex(control) + (ToFront ? 1 : -1);

            if (Extreme)
                zidx = ToFront ? 999 : -999;

            Panel.SetZIndex(control, zidx);
        }

        public static void ClearSelection()
        {
            foreach (var cc in SelectedControls)
                cc.IsMultiSelected = false;

            SelectedControls.Clear();
        }

        public static void CopyControl(CControl control)
        {
            if (control == null)
            {
                CopiedControl = null;
                return;
            }

            if (control is CFrame && (control as CFrame).Content == OutermostParent)
            {
                CopiedControl = null;
                return;
            }

            CopiedControl = control;
            CopiedControl.DataContext = control.Parent;
        }

        public static void PasteControl()
        {
            if (CopiedControl == null)
                return;

            var parent = CopiedControl.DataContext as Canvas;

            var copy = CopiedControl.GetClone;
            parent.Children.Add(copy);
            Panel.SetZIndex(copy, Panel.GetZIndex(CopiedControl) + 1);

            AddExtendedAttributes(CopiedControl, copy);

            CControl.LastFocused = copy;
            copy.Focus();
        }

        public static void PasteControls()
        {
            var selectedTemp = new CControl[SelectedControls.Count];
            SelectedControls.CopyTo(selectedTemp);

            foreach (var cc in SelectedControls)
                cc.IsMultiSelected = false;
            SelectedControls.Clear();

            foreach (var cc in selectedTemp)
            {
                if (cc == CopiedControl)
                    continue;

                var copy = cc.GetClone;
                (cc.Parent as Canvas).Children.Add(copy);
                Panel.SetZIndex(copy, Panel.GetZIndex(cc));

                AddExtendedAttributes(cc, copy);

                copy.IsMultiSelected = true;
                SelectedControls.Add(copy);
            }
        }

        public static void AddExtendedAttributes(CControl _base, CControl _derived)
        {
            //var parent = CopiedControl.DataContext as Canvas;

            //_derived.fieldname = _base.fieldname + "-Copy"; //GetFactoryName(_derived, parent);

            int idx = 1;
            for (int i = 0; i < (_base.Parent as Canvas).Children.Count; i++)
            {
                var child = (_base.Parent as Canvas).Children[i];

                if (child is CCanvas || child == _base)
                    continue;

                if ((child as CControl).fieldname == _base.fieldname + "-" + idx)
                {
                    idx++;
                    i = 0;
                }
            }

            _derived.fieldname = _base.fieldname + "-" + idx;
            _derived.xpos = _base.xpos;
            _derived.ypos = _base.ypos;

            //_derived.xpos = (int)(parent.Width / 2 - _derived.Width / 2) + "";
            //_derived.ypos = (int)(parent.Height / 2 - _derived.Height / 2) + "";

            switch (_base.GetType().Name)
            {
                case "CBitmapImagePanel":
                    var cbmpB = _base as CBitmapImagePanel;
                    var cbmp = _derived as CBitmapImagePanel;

                    cbmp.scaleimage = cbmpB.scaleimage;
                    cbmp.image = cbmpB.image;
                    cbmp.imagecolor = cbmpB.imagecolor;
                    break;

                case "CButton":
                case "CMouseOverPanelButton":
                    var cbtnB = _base as CButton;
                    var cbtn = _derived as CButton;

                    cbtn.labeltext = cbtnB.labeltext;
                    cbtn.command = cbtnB.command;
                    cbtn.textalignment = cbtnB.textalignment;
                    cbtn.font = cbtnB.font;
                    cbtn.paintbackground = cbtnB.paintbackground;
                    break;

                case "CImagePanel":
                    var cimgB = _base as CImagePanel;
                    var cimg = _derived as CImagePanel;

                    cimg.scaleimage = cimgB.scaleimage;
                    cimg.image = cimgB.image;
                    cimg.imagecolor = cimgB.imagecolor;
                    cimg.fillcolor = cimgB.fillcolor;
                    break;

                case "CLabel":
                    var clblB = _base as CLabel;
                    var clbl = _derived as CLabel;

                    clbl.labeltext = clblB.labeltext;
                    clbl.font = clblB.font;
                    clbl.textalignment = clblB.textalignment;
                    break;
            }
        }

        public static void SaveFile()
        {
            if (OriginalFileName == string.Empty)
            {
                MessageBox.Show("Open an existing resource file first.", "Save Blocked");
                return;
            }

            string contents =
                "// File was generated using CS 1.6 Resource UI Themer\n" +
                "// Created by Meowth ('https://gamebanana.com/members/1454821')\n\n";

            contents += topMostHeader + "\n{\n";

            if (ContainerControl != null)
            {
                var cchild = ContainerControl;

                contents += "\t\"" + (cchild.headerName == null || cchild.headerName == string.Empty ? cchild.fieldname : cchild.headerName) + "\"\n\t{\n";

                contents += GenerateAttr("ControlName", cchild.controlname);
                contents += GenerateAttr("fieldName", cchild.fieldname);
                contents += GenerateAttr("xpos", cchild.xpos);
                contents += GenerateAttr("ypos", cchild.ypos);
                contents += GenerateAttr("wide", cchild.wide);
                contents += GenerateAttr("tall", cchild.tall);
                contents += GenerateAttr("enabled", cchild.enabled);
                contents += GenerateAttr("visible", cchild.visible);

                contents += GenerateExtentendAttrs(cchild);
                contents += cchild.GetUnknownAttrs;

                contents += "\t}\n\n";
            }

            var childOrdered = new List<UIElement>();
            var children = new UIElement[OriginalParent.Children.Count];
            OriginalParent.Children.CopyTo(children, 0);

            foreach (UIElement child in children)
                childOrdered.Add(child);

            childOrdered.Remove(ContainerControl);
            //childOrdered.Sort(new ZIndexSorter());

            GenerateScope(ref contents, childOrdered.ToArray());

            contents += "}\n";

            var sfd = new SaveFileDialog
            {
                FileName = OriginalFileName,
                Filter = "Resource UI File|*.res",
                Title = "Save File",
                InitialDirectory = Directory.GetParent(OriginalFileName).FullName,
                AddExtension = true
            };

            sfd.FileOk += (obj, ev) =>
            {
                File.WriteAllText(sfd.FileName, contents);
                MessageBox.Show("File " + sfd.FileName + " saved successfully.", "File Saved");
            };

            sfd.ShowDialog();
        }

        public static string GenerateExtentendAttrs(CControl ccontrol)
        {
            var ret = string.Empty;

            switch (ccontrol.controlname)
            {
                case "BitmapImagePanel":
                    var bmpnl = ccontrol as CBitmapImagePanel;
                    ret += GenerateAttr("image", bmpnl.image);
                    ret += GenerateAttr("imagecolor", bmpnl.imagecolor);
                    break;

                case "Button":
                case "MouseOverPanelButton":
                    var btn = ccontrol as CButton;
                    ret += GenerateAttr("labelText", btn.labeltext);
                    ret += GenerateAttr("command", btn.command);
                    ret += GenerateAttr("textAlignment", btn.textalignment);
                    ret += GenerateAttr("font", btn.font);
                    ret += GenerateAttr("paintbackground", btn.paintbackground);
                    break;

                case "ImagePanel":
                    var imgpnl = ccontrol as CImagePanel;
                    ret += GenerateAttr("image", imgpnl.image);
                    ret += GenerateAttr("scaleimage", imgpnl.scaleimage);
                    break;

                case "Label":
                    var lbl = ccontrol as CLabel;
                    ret += GenerateAttr("labelText", lbl.labeltext);
                    ret += GenerateAttr("font", lbl.font);
                    ret += GenerateAttr("textAlignment", lbl.textalignment);
                    break;
            }

            return ret;
        }

        public static void GenerateScope(ref string retVal, UIElement[] controls)
        {
            Array.Sort(controls, new ZIndexSorter());

            foreach (UIElement child in controls)
            {
                if (child is CCanvas)
                {
                    var cnv = child as CCanvas;
                    var arr = new UIElement[cnv.Children.Count];
                    cnv.Children.CopyTo(arr, 0);

                    GenerateScope(ref retVal, arr);
                    continue;
                }

                var cchild = child as CControl;

                if (cchild.Discard)
                    continue;

                retVal += "\t\"" + (cchild.headerName == null || cchild.headerName == string.Empty ? cchild.fieldname : cchild.headerName) + "\"\n\t{\n";

                retVal += GenerateAttr("ControlName", cchild.controlname);
                retVal += GenerateAttr("fieldName", cchild.fieldname);
                retVal += GenerateAttr("xpos", cchild.xpos);
                retVal += GenerateAttr("ypos", cchild.ypos);
                retVal += GenerateAttr("wide", cchild.wide);
                retVal += GenerateAttr("tall", cchild.tall);
                retVal += GenerateAttr("enabled", cchild.enabled);
                retVal += GenerateAttr("visible", cchild.visible);

                retVal += GenerateExtentendAttrs(cchild);
                retVal += cchild.GetUnknownAttrs;

                retVal += "\t}\n\n";

                if (cchild is CFrame)
                    GenerateScope(ref retVal, (cchild as CFrame).Children);
            }
        }

        public static string GenerateAttr(string attr, string val)
        {
            if (val == null || val.Trim().Length == 0)
                return string.Empty;

            return "\t\t" + "\"" + attr + "\"\t\t" + "\"" + val + "\"" + "\n";
        }

        public static CControl[] ParseFile(string resPath, bool UpdateHeader = true)
        {
            if (!File.Exists(resPath))
            {
                MessageBox.Show("No such file: " + resPath, "File Not Found");
                return new CControl[0];
            }

            var rootDir = resPath;
            while (Directory.GetDirectoryRoot(rootDir) != rootDir && !(rootDir.EndsWith("cstrike") || rootDir.EndsWith("czero")))
                rootDir = Directory.GetParent(rootDir).FullName;

            if (!(rootDir.EndsWith("cstrike") || rootDir.EndsWith("czero")))
            {
                MessageBox.Show("Target directory is not a CS directory. Must be cstrike or czero.", "Open Blocked");
                return new CControl[0];
            }

            HomeDir = rootDir;
            string[] lines = RemoveOutermostScope(RemoveComments(File.ReadAllLines(resPath)), UpdateHeader);

            if (lines.Length < 3)
                return new CControl[0];

            var ccontrolList = new List<CControl>();

            for (int idx = 0; idx < lines.Length; idx++)
            {
                var ccontrol = GetScope(lines, ref idx);

                if (ccontrol == null)
                    break;

                ccontrolList.Add(ccontrol);
                //Panel.SetZIndex(ccontrol, zindex++);
            }

            MaxZ = 0;
            foreach (var cc in ccontrolList)
                Panel.SetZIndex(cc, MaxZ++);
            MaxZ++;

            return ccontrolList.ToArray();
        }

        public static void ForceFrameUpdate(CControl control)
        {
            // Force relocate controls (stuck on 0,0 during first-run) 
            Canvas.SetLeft(control, GetOffset(control, control.xpos));
            Canvas.SetTop(control, GetOffset(control, control.ypos, false));
        }

        private static string[] RemoveOutermostScope(string[] lines, bool UpdateHeader = true)
        {
            if (UpdateHeader)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    if (line.StartsWith("//"))
                        continue;

                    if (line.StartsWith("\""))
                    {
                        topMostHeader = line;
                        break;
                    }
                }
            }

            int startIndex = -1;
            int endIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (startIndex != -1 && endIndex != -1)
                    break;

                if (startIndex == -1 && lines[i].Trim().StartsWith("{"))
                    startIndex = i + 1;

                else if (endIndex == -1 && lines[lines.Length - i - 1].Trim().StartsWith("}"))
                    endIndex = lines.Length - i;
            }

            var newlines = new string[endIndex - startIndex];
            Array.Copy(lines, startIndex, newlines, 0, endIndex - startIndex);

            return newlines;
        }

        public static void BatchMove(int diffX, int diffY)
        {
            foreach (var cc in SelectedControls.Where(p => p != CControl.LastFocused))
            {
                if (cc.Locked || cc.enabled != "1")
                    continue;

                if (!(cc.xpos.StartsWith("c") || cc.xpos.StartsWith("r")))
                    cc.xpos = (cc.GetActualX + diffX) + "";

                if (!cc.ypos.StartsWith("c"))
                    cc.ypos = (cc.GetActualY + diffY) + "";
            }
        }

        public static string[] RemoveComments(string[] lines)
        {
            var newlines = new List<string>();

            foreach (var line in lines)
            {
                var nl = line.Trim();

                if (!nl.StartsWith("//"))
                    newlines.Add(nl);
            }

            return newlines.ToArray();
        }

        private static CControl GetScope(string[] lines, ref int idx)
        {
            CControl control = null;
            var tempHeader = string.Empty;

            while (idx < lines.Length && !lines[idx].Contains('{'))
            {
                tempHeader += lines[idx];
                idx++;
            }
            idx++;

            while (idx < lines.Length && !lines[idx].Contains('}'))
            {
                //var line = lines[idx].Trim().Replace("\"", "");
                //var tok = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                //if (tok.Length < 2)
                //{
                //    idx++;
                //    continue;
                //}

                var line = lines[idx].Trim();

                var attr = string.Empty;
                var val = string.Empty;

                int i = 0;
                while (i < line.Length && line[i] != '"')
                    i++;
                i++;

                while (i < line.Length && line[i] != '"')
                {
                    attr += line[i];
                    i++;
                }
                i++;

                while (i < line.Length && line[i] != '"')
                    i++;
                i++;

                if (i >= line.Length)
                {
                    idx++;
                    continue;
                }

                while (i < line.Length && line[i] != '"')
                {
                    val += line[i];
                    i++;
                }

                attr = attr.Trim();

                if (line.ToLower().Contains("controlname") && control == null)
                {
                    var type = GetControlType(val);
                    var ob = Activator.CreateInstance(type);

                    control = ob as CControl;
                    control.controlname = val;

                    //control.headerName = tempHeader.Trim().Replace("\"", "");
                }
                else if (control != null && val.Length > 0)
                {
                    var type = control.GetType();

                    try
                    {
                        var propinfo = type.GetProperty(attr.ToLower());

                        if (propinfo != null)
                            propinfo.SetValue(control, val, null);
                        else
                            control.UnknownAttrs.Add(lines[idx]);

                    }
                    catch (ArgumentException)
                    {
                        control.UnknownAttrs.Add(lines[idx]);
                        //ApplyToProperty(type.GetProperty(tok[0]), control, tok[1]);
                    }
                }

                idx++;
            }

            if (control != null)
            {
                var header = tempHeader.Trim().Replace("\"", "");
                if (header != control.fieldname)
                    control.headerName = header;
                else
                    control.headerName = string.Empty;
            }

            return control;
        }

        public static string ReplaceHyphenToUnderscore(string content, bool reverse = false)
        {
            char origChar = reverse ? '_' : '-';
            char replacementChar = reverse ? '-' : '_';

            return content.Replace(origChar, replacementChar);
        }

        public static void ApplyToProperty(PropertyInfo property, object owner, object value)
        {
            switch (property.GetType().Name)
            {
                case "double":
                    property.SetValue(owner, double.Parse(value.ToString()), null);
                    break;

                case "string":
                    property.SetValue(owner, value, null);
                    break;
            }
        }

        public static int GetOffset(CControl control, string input, bool xOnly = true)
        {
            if (input == null)
                return 0;

            var success = int.TryParse(input, out int inputval);

            //SetParent(control);

            if (success)
                return inputval;

            //if (!(control.Parent is Canvas parent))
            //    parent = OriginalParent;

            var parent = control.Parent as Canvas;

            if (parent == null)
                return 0;

            if (input.StartsWith("c"))
            {
                input = input.Replace("c", "");
                var successc = int.TryParse(input, out int centeroffset);

                if (successc)
                    return (int)((xOnly ? parent.Width : parent.Height) / 2 + centeroffset);
            }
            else if (xOnly && input.StartsWith("r"))
            {
                input = input.Replace("r", "");
                var successr = int.TryParse(input, out int rightoffset);

                if (successr)
                    return (int)(parent.Width - rightoffset);
            }

            return 0;
        }

        public static void SetParent(CControl control)
        {
            if (control == null)
                return;

            var x = control.xpos + control.ypos;
            var Parent = control.Parent;

            //if (control.Parent != null && control.OldParent != null)
            //MessageBox.Show("Current Parent: " + (control.Parent as Canvas).Name + "\n" +
            //                "Reserved Parent: " + (control.OldParent as Canvas).Name);

            //if (Parent == OriginalParent && control.OldParent == OriginalParent)
            //    return;

            if (x.Contains('c') || x.Contains('r'))
            {
                if (Parent != null && Parent != OriginalParent)
                {
                    (Parent as Canvas).Children.Remove(control);
                    OriginalParent.Children.Add(control);
                    //control.visible = "1";
                }

                //if (Parent is Canvas)
                //{
                //    if (!OriginalParent.Children.Contains(control))
                //    {
                //        (Parent as Canvas).Children.Remove(control);

                //        OriginalParent.Children.Add(control);
                //        control.Visibility = Visibility.Visible;
                //    }
                //}
            }
            else
            {
                if (control.Parent is Canvas && control.Parent != control.OldParent)
                {
                    if (control.OldParent != null && !control.OldParent.Children.Contains(control))
                    {
                        (control.Parent as Canvas).Children.Remove(control);

                        control.OldParent.Children.Add(control);
                        //control.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public static CControl GetControlByName(Canvas parent, string fieldName)
        {
            foreach (CControl child in parent.Children)
            {
                if (child.fieldname == fieldName)
                    return child;
            }

            return null;
        }

        public static int StrToInt(CControl ctl, string input)
        {
            if (ctl == null)
                return Util.ErrorInt;

            int.TryParse(input, out int x);

            return x;
        }

        public static BitmapSource ConvertTGAToPNG(string imagepath)
        {
            BitmapSource imgsrc;

            var temp = imagepath.Trim();

            if (temp.Length == 0)
                imgsrc = null;
            else
            {
                var bmpFile = HomeDir + "\\" + temp + ".bmp";

                #region TGA File Fetcher
                var tgaFile = HomeDir + "\\" + temp + ".tga";

                if (File.Exists(tgaFile))
                {
                    using (var fstream = new FileStream(tgaFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new BinaryReader(fstream))
                        imgsrc = new TgaLib.TgaImage(reader).GetBitmap();

                    //if (imgsrc != null && (imgsrc.Width > 256 || imgsrc.Height > 256))
                    //{
                    //    if (imgsrc.Width <= 276)
                    //    {
                    //        if (imgsrc.Height > 96)
                    //        {
                    //            MessageBox.Show(imagepath + ": " + "Maximum allowed height for width:276 is 96.", "Image Update Blocked");
                    //            return null;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show(imagepath + ": " + "Maximum allowed size for TARGA is 256x256.", "Image Update Blocked");
                    //        return null;
                    //    }

                    //    if (imgsrc.Height <= 276)
                    //    {
                    //        if (imgsrc.Width > 96)
                    //        {
                    //            MessageBox.Show(imagepath + ": " + "Maximum allowed width for height:276 is 96.", "Image Update Blocked");
                    //            return null;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show(imagepath + ": " + "Maximum allowed size for TARGA is 256x256.", "Image Update Blocked");
                    //        return null;
                    //    }

                    //    //MessageBox.Show(imagepath + ": " + "Maximum allowed size for TARGA is 256x256", "Image Update Blocked");
                    //    //return null;
                    //}
                }
                #endregion

                else if (File.Exists(bmpFile))
                {
                    try
                    {
                        imgsrc = new BitmapImage(new Uri(bmpFile, UriKind.Absolute));
                    }
                    catch
                    {
                        MessageBox.Show("Cannot load image " + temp + ".bmp", "Error Occured");
                        imgsrc = null;
                    }
                }
                else
                    imgsrc = null;
            }

            return imgsrc;
        }

        public static Point GetAligmnent(FormattedText format, double Width, double Height, TextAlign alignment, double padding)
        {
            var pt = new Point();

            switch (alignment)
            {
                case TextAlign.north_west:
                    pt.X = padding;
                    pt.Y = padding;
                    break;

                case TextAlign.north:
                    pt.X = Width / 2 - format.Width / 2;
                    pt.Y = padding;
                    break;

                case TextAlign.north_east:
                    pt.X = Width - format.Width - padding;
                    pt.Y = padding;
                    break;

                case TextAlign.west:
                    pt.X = padding;
                    pt.Y = Height / 2 - format.Height / 2;
                    break;

                case TextAlign.center:
                    pt.X = Width / 2 - format.Width / 2;
                    pt.Y = Height / 2 - format.Height / 2;
                    break;

                case TextAlign.east:
                    pt.X = Width - format.Width - padding;
                    pt.Y = Height / 2 - format.Height / 2;
                    break;

                case TextAlign.south_west:
                    pt.X = padding;
                    pt.Y = Height - format.Height - padding;
                    break;

                case TextAlign.south:
                    pt.X = Width / 2 - format.Width / 2;
                    pt.Y = Height - format.Height - padding;
                    break;

                case TextAlign.south_east:
                    pt.X = Width - format.Width - padding;
                    pt.Y = Height - format.Height - padding;
                    break;
            }

            return pt;
        }

        public static string CreateDirectory(string path)
        {
            bool isFile = File.Exists(path);
            string msg = "SUCCESS";

            try
            {
                if (isFile)
                    Directory.CreateDirectory(path);
                else
                    Directory.CreateDirectory(Directory.GetParent(path).FullName);
            }
            catch (Exception e) { msg = e.Message; }

            return msg;
        }

        public static bool DesignMode { get => LicenseManager.UsageMode == LicenseUsageMode.Designtime; }

        public static Color StringToColor(string colorstr)
        {
            var clr = colorstr.Trim();

            if (clr == string.Empty)
                return EmptyColor;

            var rgba = clr.Split();

            if (rgba.Length < 4)
                return EmptyColor;

            byte.TryParse(rgba[0], out byte r);
            byte.TryParse(rgba[1], out byte g);
            byte.TryParse(rgba[2], out byte b);
            byte.TryParse(rgba[3], out byte a);

            return Color.FromArgb(a, r, g, b);
        }

        public static string GetFactoryName(CControl control, Canvas parent)
        {
            int cardinal = 1;

            foreach (UIElement child in parent.Children)
            {
                if (!(child is CControl))
                    continue;

                if ((child as CControl).fieldname == GetControlName(control.GetType()) + cardinal)
                    cardinal++;
            }

            return GetControlName(control.GetType()) + cardinal;
        }

        public static bool VerifyHeaderUniqueness(string header, CControl control)
        {
            if (!(control.Parent is Canvas) || header == null || header == string.Empty)
                return true;

            foreach (UIElement child in (control.Parent as Canvas).Children)
            {
                if (!(child is CControl))
                    continue;

                var ch = child as CControl;

                if (ch.headerName == header && child != control)
                    return false;
            }

            return true;
        }

        public static Type GetControlType(string type)
        {
            foreach (var c in controlTypes)
            {
                if (c.Key == type)
                    return c.Value;
            }

            return typeof(CUnknown);
        }

        public static string GetControlName(Type type)
        {
            foreach (var c in controlTypes)
            {
                if (c.Value == type)
                    return c.Key;
            }

            return "UnknownType";
        }

        public static Comparison ComparePoints(System.Windows.Point A, System.Windows.Point B)
        {
            if (A == B)
                return Comparison.EQUAL;

            if (A.X > B.X && (A.Y > B.Y || A.Y < B.Y))
                return Comparison.GREATER;

            if (A.X < B.X && (A.Y < B.Y || A.Y > B.Y))
                return Comparison.LESSER;

            return Comparison.UNKNOWN;
        }

        public static bool IsPointNegative(System.Windows.Point A)
        {
            return A.X < 0 || A.Y < 0;
        }
    }

    public class ZIndexSorter : IComparer<UIElement>
    {
        public int Compare(UIElement x, UIElement y)
        {
            return Panel.GetZIndex(x).CompareTo(Panel.GetZIndex(y));
        }
    }
}
