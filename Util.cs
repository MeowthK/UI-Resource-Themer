using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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
        //public static readonly TextBlock TitleFont = new TextBlock();

        public static string HomeDir { get; set; }
        public static string WorkDir { get; set; }
        public static string OriginalFileName { get; set; } = string.Empty;
        public static CControl ContainerControl { get; set; } = null;
        public static LoadingDialog LoadDialog { get; set; }

        public static readonly Color EmptyColor = Color.FromArgb(0, 0, 0, 0);
        public static readonly double EmSize = 10.0;
        public static readonly double LabelMargin = 6.0;
        public static Canvas OutermostParent { get; set; }
        public static Canvas OriginalParent { get; set; }

        public static int ErrorInt = -32767;

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

        public static void ArrangeControl(CControl control, bool ToFront = true, bool Extreme = false)
        {
            if (control == null || !(control.Parent is Canvas parent))
                return;

            if (control is CFrame)
                return;

            var zidx = Panel.GetZIndex(control) + (ToFront ? 1 : -1);

            if (Extreme)
                zidx = ToFront ? 999 : -999;

            Panel.SetZIndex(control, zidx);
        }

        public static void SaveFile()
        {
            if (OriginalFileName == string.Empty)
            {
                MessageBox.Show("Open an existing resource file first.", "Save Blocked");
                return;
            }

            string contents = topMostHeader + "\n{\n";

            if (ContainerControl != null)
            {
                var cchild = ContainerControl;

                contents += "\t\"" + (cchild.headerName == null || cchild.headerName == string.Empty ? cchild.fieldname : cchild.headerName) + "\"\n\t{\n";

                contents += "\t\t" + GenerateAttr("ControlName", cchild.controlname) + "\n";
                contents += "\t\t" + GenerateAttr("fieldName", cchild.fieldname)     + "\n";
                contents += "\t\t" + GenerateAttr("xpos", cchild.xpos)               + "\n";
                contents += "\t\t" + GenerateAttr("ypos", cchild.ypos)               + "\n";
                contents += "\t\t" + GenerateAttr("wide", cchild.wide)               + "\n";
                contents += "\t\t" + GenerateAttr("tall", cchild.tall)               + "\n";

                contents += GenerateExtentendAttrs(cchild);
                contents += cchild.GetUnknownAttrs;

                contents += "\t}\n\n";
            }

            var childOrdered = new List<UIElement>();

            foreach (UIElement child in OutermostParent.Children)
                childOrdered.Add(child);

            childOrdered.Sort(new ZIndexSorter());

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
                MessageBox.Show("Saved: " + sfd.FileName);
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
                    ret += "\t\t" + GenerateAttr("image", bmpnl.image)           + "\n";
                    ret += "\t\t" + GenerateAttr("imagecolor", bmpnl.imagecolor) + "\n";
                    break;

                case "Button":
                    var btn = ccontrol as CButton;
                    ret += "\t\t" + GenerateAttr("labelText", btn.labeltext)             + "\n";
                    ret += "\t\t" + GenerateAttr("command", btn.command)                 + "\n";
                    ret += "\t\t" + GenerateAttr("textAlignment", btn.textalignment)     + "\n";
                    ret += "\t\t" + GenerateAttr("font", btn.font)                       + "\n";
                    ret += "\t\t" + GenerateAttr("paintbackground", btn.paintbackground) + "\n";
                    break;

                case "ImagePanel":
                    var imgpnl = ccontrol as CImagePanel;
                    ret += "\t\t" + GenerateAttr("image", imgpnl.image)           + "\n";
                    ret += "\t\t" + GenerateAttr("scaleimage", imgpnl.scaleimage) + "\n";
                    break;

                case "Label":
                    var lbl = ccontrol as CLabel;
                    ret += "\t\t" + GenerateAttr("labelText", lbl.labeltext)         + "\n";
                    ret += "\t\t" + GenerateAttr("font", lbl.font)                   + "\n";
                    ret += "\t\t" + GenerateAttr("textAlignment", lbl.textalignment) + "\n";
                    break;
            }

            return ret;
        }

        public static void GenerateScope(ref string retVal, UIElement[] controls)
        {
            foreach (UIElement child in controls)
            {
                if (child is CCanvas)
                    continue;

                var cchild = child as CControl;

                retVal += "\t\"" + (cchild.headerName == null || cchild.headerName == string.Empty ? cchild.fieldname : cchild.headerName) + "\"\n\t{\n";

                retVal += "\t\t" + GenerateAttr("ControlName", cchild.controlname) + "\n";
                retVal += "\t\t" + GenerateAttr("fieldName", cchild.fieldname)     + "\n";
                retVal += "\t\t" + GenerateAttr("xpos", cchild.xpos)               + "\n";
                retVal += "\t\t" + GenerateAttr("ypos", cchild.ypos)               + "\n";
                retVal += "\t\t" + GenerateAttr("wide", cchild.wide)               + "\n";
                retVal += "\t\t" + GenerateAttr("tall", cchild.tall)               + "\n";
                retVal += "\t\t" + GenerateAttr("enabled", cchild.enabled)         + "\n";
                retVal += "\t\t" + GenerateAttr("visible", cchild.visible)         + "\n";

                retVal += GenerateExtentendAttrs(cchild);
                retVal += cchild.GetUnknownAttrs;

                retVal += "\t}\n\n";

                if (cchild is CFrame)
                    GenerateScope(ref retVal, (cchild as CFrame).Children);
            }
        }

        public static string GenerateAttr(string attr, string val)
        {
            return "\"" + attr + "\"\t\t" + "\"" + val + "\"";
        }

        public static CControl[] ParseFile(string resPath)
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
            string[] lines = RemoveOutermostScope(RemoveComments(File.ReadAllLines(resPath)));

            if (lines.Length < 3)
                return new CControl[0];

            var ccontrolList = new List<CControl>();

            int zindex = 0;
            for (int idx = 0; idx < lines.Length; idx++)
            {
                var ccontrol = GetScope(lines, ref idx);

                if (ccontrol == null)
                    break;

                ccontrolList.Add(ccontrol);
                Panel.SetZIndex(ccontrol, zindex++);
            }

            return ccontrolList.ToArray();
        }

        public static void ForceFrameUpdate(CControl control)
        {
            // Force relocate controls (stuck on 0,0 during first-run) 
            Canvas.SetLeft(control, GetOffset(control, control.xpos));
            Canvas.SetTop(control, GetOffset(control, control.ypos, false));
        }

        private static string[] RemoveOutermostScope(string[] lines)
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
                val = val.Trim();

                if (line.ToLower().Contains("controlname") && control == null)
                {
                    var type = GetControlType(val);
                    var ob = Activator.CreateInstance(type);

                    control = ob as CControl;
                    control.controlname = val;
                    control.headerName = tempHeader.Trim().Replace("\"", "");
                }
                else if (control != null)
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

            if (success)
            {
                SetParent(control);
                return inputval;
            }

            SetParent(control);

            if (!(control.Parent is Canvas parent))
                parent = OriginalParent;

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

            if (x.Contains('c') || x.Contains('r'))
            {
                if (Parent is CCanvas)
                {
                    if (!OriginalParent.Children.Contains(control))
                    {
                        (Parent as CCanvas).Children.Remove(control);

                        OriginalParent.Children.Add(control);
                        control.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if (control.Parent is Canvas && control.Parent != control.OldParent)
                {
                    if (control.OldParent != null && !control.OldParent.Children.Contains(control))
                    {
                        (control.Parent as Canvas).Children.Remove(control);

                        control.OldParent.Children.Add(control);
                        control.Visibility = Visibility.Visible;
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

        public static bool VerifyNameuniqueness(string name, CControl control)
        {
            foreach (UIElement child in (control.Parent as Canvas).Children)
            {
                if (!(child is CControl))
                    continue;

                var ch = child as CControl;

                if (ch.fieldname == name && child != control)
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
