using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
    ///     <MyNamespace:CPanel/>
    ///
    /// </summary>
    /// 
    public enum DefaultPanels
    {
        TopLeftPanel,
        TopRightPanel,
        BottomLeftPanel,
        BottomRightPanel,
        GapPanel,
        DontCopyMyNameBecauseImJustAPlaceholder
    }

    public class CPanel : CFrame
    {
        private Brush brushBG = Util.DefaultBG;
        private DefaultPanels defaultPanel = DefaultPanels.DontCopyMyNameBecauseImJustAPlaceholder;

        public Brush BrushBG { get => brushBG; set { brushBG = value; InvalidateVisual(); } }

        static CPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CPanel), new FrameworkPropertyMetadata(typeof(CPanel)));
        }

        public CPanel()
        {
            Content.DrawWireframe = false;
            BoxColor = Util.EmptyColor;

            NameChanged += NameChange;
            Unloaded += (o, e) => NameChanged -= NameChange;
        }

        private void NameChange(object sender, EventArgs e)
        {
            foreach (var en in Enum.GetValues(typeof(DefaultPanels)))
            {
                if (Enum.GetName(typeof(DefaultPanels), en) == fieldname)
                {
                    defaultPanel = (DefaultPanels)en;
                    InvalidateVisual();
                    return;
                }
            }

            defaultPanel = DefaultPanels.DontCopyMyNameBecauseImJustAPlaceholder;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (defaultPanel == DefaultPanels.DontCopyMyNameBecauseImJustAPlaceholder)
            {
                drawingContext.DrawRectangle(brushBG, Util.EmptyPen, new Rect(RenderSize));
                base.OnRender(drawingContext);
                return;
            }

            #region Simulate default panel type's appearance
            BitmapSource tmpbmp = null;

            switch (defaultPanel)
            {
                case DefaultPanels.TopLeftPanel:
                    tmpbmp = Util.ConvertTGAToPNG(@"\gfx\vgui\round_corner_nw");
                    break;

                case DefaultPanels.TopRightPanel:
                    tmpbmp = Util.ConvertTGAToPNG(@"\gfx\vgui\round_corner_ne");
                    break;

                case DefaultPanels.BottomLeftPanel:
                    tmpbmp = Util.ConvertTGAToPNG(@"\gfx\vgui\round_corner_sw");
                    break;

                case DefaultPanels.BottomRightPanel:
                    tmpbmp = Util.ConvertTGAToPNG(@"\gfx\vgui\round_corner_se");
                    break;
            }

            if (tmpbmp != null)
            {
                tmpbmp.Freeze();
                drawingContext.DrawImage(tmpbmp, new Rect(RenderSize));
            }

            base.OnRender(drawingContext);
            #endregion
        }
    }
}
