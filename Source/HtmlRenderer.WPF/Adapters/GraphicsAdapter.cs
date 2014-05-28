// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WPF.Utilities;

namespace HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WinForms Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// The wrapped WinForms graphics object
        /// </summary>
        private readonly DrawingContext _g;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the WPF graphics object to use</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(DrawingContext g, bool releaseGraphics = false)
            : base(WpfAdapter.Instance)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _releaseGraphics = releaseGraphics;
        }

        public override RBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(color1), Utils.Convert(color2), angle));
        }

        public override RRect GetClip()
        {
            // TODO:a handle clip
            //            var clip = _g.ClipBounds;
            //            return Utils.Convert(clip);
            return new RRect(0, 0, 9999, 9999);
        }

        public override void SetClipReplace(RRect rect)
        {
            // TODO:a handle clip
            //            _g.SetClip(Utils.Convert(rect), CombineMode.Replace);
        }

        public override void SetClipExclude(RRect rect)
        {
            // TODO:a handle clip
            //            _g.SetClip(Utils.Convert(rect), CombineMode.Exclude);
        }

        public override Object SetAntiAliasSmoothingMode()
        {
            //            var prevMode = _g.SmoothingMode;
            //            _g.SmoothingMode = SmoothingMode.AntiAlias;
            //            return prevMode;
            return null;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        {
            if (prevMode != null)
            {
                // TODO:a handle smoothing mode
                //                _g.SmoothingMode = (SmoothingMode)prevMode;
            }
        }

        public override RSize MeasureString(string str, RFont font)
        {
            var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, Brushes.Red);
            return new RSize(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
        }

        public override RSize MeasureString(string str, RFont font, double maxWidth, out int charFit, out int charFitWidth)
        {
            var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, Brushes.Red);
            charFit = str.Length;
            charFitWidth = (int)formattedText.Width;
            return new RSize(formattedText.Width, formattedText.Height);
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var colorConv = ((BrushAdapter)_adapter.GetSolidBrush(color)).Brush;

            var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, colorConv);
            _g.DrawText(formattedText, Utils.Convert(point));
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new ImageBrush(((ImageAdapter)image).Image);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.Tile;
            brush.Viewport = Utils.Convert(dstRect);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Transform = new TranslateTransform(translateTransformLocation.X, translateTransformLocation.Y);
            brush.Freeze();
            return new BrushAdapter(brush);
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            if (_releaseGraphics)
                _g.Close();
        }


        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            _g.DrawLine(((PenAdapter)pen).CreatePen(), new Point(x1, y1), new Point(x2, y2));
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            var pen2 = ((PenAdapter)pen).CreatePen();
            _g.DrawRectangle(null, pen2, new Rect(x, y, width, height));
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            var brush2 = ((BrushAdapter)brush).Brush;
            _g.DrawRectangle(brush2, null, new Rect(x, y, width, height));
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            // TODO:a handle image source
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            _g.DrawGeometry(null, ((PenAdapter)pen).CreatePen(), ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.DrawGeometry(((BrushAdapter)brush).Brush, null, ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                var g = new StreamGeometry();
                using (var context = g.Open())
                {
                    context.BeginFigure(Utils.Convert(points[0]), true, true);
                    for (int i = 1; i < points.Length; i++)
                        context.LineTo(Utils.Convert(points[i]), false, true);
                }
                g.Freeze();

                _g.DrawGeometry(((BrushAdapter)brush).Brush, null, g);
            }
        }

        #endregion
    }
}