using System;
using System.Globalization;
using System.IO;
using System.Security.RightsManagement;
using System.Windows;
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
    ///     <MyNamespace:CButton/>
    ///
    /// </summary>
    public class CButton : CControl
    {
        #region Private Fields
        private string labeltxt = string.Empty, dictionaryText = string.Empty;
        private string txtalignment = "west";
        private string fnt = "Verdana";
        private string paintbg = "1";
        private readonly Brush[] bgbrushes = { Brushes.Transparent, Util.DefaultBG };
        #endregion

        #region Public Fields
        public string labeltext
        {
            get => labeltxt;
            set
            {
                if (labeltxt != value)
                {
                    labeltxt = value;

                    if (labeltxt.StartsWith("#"))
                    {
                        var moddir = Path.GetFileName(Util.HomeDir);
                        var dictPath = Util.HomeDir + "\\resource\\" + moddir + "_english.txt";

                        if (Util.HomeDir != string.Empty && File.Exists(dictPath))
                        {
                            string[] contents = File.ReadAllLines(dictPath);
                            var tempname = labeltxt.Replace("#", "");
                            dictionaryText = string.Empty;

                            foreach (var line in contents)
                            {
                                if (line.Trim().StartsWith("//"))
                                    continue;

                                var qualifer = string.Empty;

                                var hn = line.Trim();

                                int i = 0;
                                while (i < hn.Length && hn[i] != '"')
                                    i++;
                                i++;

                                while (i < hn.Length && hn[i] != '"')
                                {
                                    qualifer += hn[i];
                                    i++;
                                }
                                i++;

                                if (tempname.ToLower() == qualifer.ToLower())
                                {
                                    var tempdict = string.Empty;

                                    while (i < hn.Length && hn[i] != '"')
                                        i++;
                                    i++;

                                    while (i < hn.Length && hn[i] != '"')
                                    {
                                        tempdict += hn[i];
                                        i++;
                                    }

                                    var tempdict2 = string.Empty;

                                    for (int j = 0; j < tempdict.Length; j++)
                                    {
                                        if (tempdict[j] == '&' && j + 1 < tempdict.Length && tempdict[j + 1] != '&')
                                            continue;

                                        if (tempdict[j] == '\\' && j + 1 < tempdict.Length)
                                        {
                                            switch (tempdict[j + 1])
                                            {
                                                case 'n':
                                                    tempdict2 += "\n";
                                                    break;

                                                case 't':
                                                    tempdict2 += "\t";
                                                    break;
                                            }

                                            j++;
                                            continue;
                                        }

                                        tempdict2 += tempdict[j];
                                    }

                                    dictionaryText = tempdict2;
                                } 
                            }
                        }
                    }
                    else if (dictionaryText != string.Empty)
                        dictionaryText = string.Empty;

                    InvalidateVisual();
                }
            }
        }

        public string command { get; set; }
        public string textalignment
        {
            get => txtalignment;
            set
            {
                if (txtalignment != value)
                {
                    txtalignment = value;
                    InvalidateVisual();
                }
            }
        }
        public string font
        {
            get => fnt;
            set
            {
                if (fnt != value)
                {
                    fnt = value;
                    InvalidateVisual();
                }
            }
        }
        public string paintbackground
        {
            get => paintbg;
            set
            {
                if (paintbg != value)
                {
                    paintbg = value;
                    BoxColor = paintbg != "0" ? (Util.DefaultFG as SolidColorBrush).Color : Util.EmptyColor;
                    InvalidateVisual();
                }
            }
        }
        #endregion

        static CButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CButton), new FrameworkPropertyMetadata(typeof(CButton)));
        }

        public CButton()
        {
            BoxColor = (Util.DefaultFG as SolidColorBrush).Color;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var cultureinfo = CultureInfo.CurrentCulture;
            var flowdirection = FlowDirection.LeftToRight;
            var typeface = new Typeface(fnt);
            var emsize = Util.EmSize;
            var foreground = Util.DefaultFG;
            var pt = new Point();
            var label = dictionaryText != string.Empty ? dictionaryText : labeltext;

            var format = new FormattedText(label, cultureinfo, flowdirection, typeface, emsize, foreground)
            { MaxTextWidth = Width };

            foreach (var e in Enum.GetValues(typeof(TextAlign)))
            {
                if (textalignment == Enum.GetName(typeof(TextAlign), e).Replace('_', '-'))
                {
                    pt = Util.GetAligmnent(format, Width, Height, (TextAlign)e, Util.LabelMargin);
                    break;
                }
            }

            drawingContext.DrawRectangle(bgbrushes[paintbg != "0" ? 1 : 0], Util.EmptyPen, new Rect(RenderSize));
            drawingContext.DrawText(format, pt);
            base.OnRender(drawingContext);
        }
    }
}
