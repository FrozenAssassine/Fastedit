using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Fastedit.Controls.Textbox
{
    //Code inspired from https://github.com/JasonStein/Notepads/blob/master/src/Notepads/Controls/TextEditor/TextEditorCore.LineNumbers.cs
    public class Linenumbers
    {
        TextControlBox tcb = null;
        RichEditBox textbox = null;
        private readonly IList<TextBlock> RenderedLineNumbers = new List<TextBlock>();
        private readonly Dictionary<string, double> _miniRequisiteIntegerTextRenderingWidthCache = new Dictionary<string, double>();

        public Linenumbers(TextControlBox tb, RichEditBox textb)
        {
            tcb = tb;
            textbox = textb;
        }

        //Linenumbers
        public void ShowLinenumbers()
        {
            if (tcb.LineNumberGrid != null)
            {
                ResetLinenumberCanvas();
                UpdateLinenumberRendering();
            }
        }
        public void HideLinenumbers()
        {
            if (tcb.LineNumberGrid != null)
            {
                for (int i = 0; i < RenderedLineNumbers.Count; i++)
                {
                    RenderedLineNumbers[i].Visibility = Visibility.Collapsed;
                }

                tcb.LineNumberGrid.BorderThickness = new Thickness(0, 0, 0, 0);
                tcb.LineNumberGrid.Margin = new Thickness(0, 0, 0, 0);
                tcb.LineNumberGrid.Width = .0f;
            }
        }
        public void ResetLinenumberCanvas()
        {
            if (tcb.LineNumberGrid != null)
            {
                tcb.LineNumberGrid.Margin = new Thickness(0, 0, (-1 * tcb.Padding.Left) + 1, 0);
                tcb.LineNumberGrid.Clip = new RectangleGeometry
                {
                    Rect = new Rect(
                        0,
                        tcb.Padding.Top,
                        tcb.LineNumberGrid.ActualWidth,
                        Math.Clamp(tcb.LineNumberGrid.ActualHeight - (tcb.Padding.Top + tcb.Padding.Bottom), .0f, Double.PositiveInfinity))
                };
            }
        }
        public void UpdateLinenumberRendering()
        {
            if (!tcb.ShowLineNumbers) return;

            if (tcb.MainContentScrollViewer != null)
            {
                var startRange = textbox.Document.GetRangeFromPoint(
                    new Point(tcb.MainContentScrollViewer.HorizontalOffset, tcb.MainContentScrollViewer.VerticalOffset),
                    PointOptions.ClientCoordinates);

                var endRange = textbox.Document.GetRangeFromPoint(
                    new Point(tcb.MainContentScrollViewer.HorizontalOffset + tcb.MainContentScrollViewer.ViewportWidth,
                        tcb.MainContentScrollViewer.VerticalOffset + tcb.MainContentScrollViewer.ViewportHeight),
                    PointOptions.ClientCoordinates);

                var document = tcb.GetLineNumberContent;
                document = document.Append("\n");

                Dictionary<int, Rect> lineNumberTextRenderingPositions = GetLinenumberPos(document, startRange, endRange);

                var minLineNumberTextRenderingWidth = CalculateMinimumTextRenderingWidth(tcb.FontFamily,
                    textbox.FontSize, (document.Length - 1).ToString().Length) + 10;

                DoRenderLineNumbers(lineNumberTextRenderingPositions, minLineNumberTextRenderingWidth);
            }
        }
        public Size GetTextSize(FontFamily font, double fontSize, string text)
        {
            var tb = new TextBlock { Text = text, FontFamily = font, FontSize = fontSize };
            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return tb.DesiredSize;
        }
        public double CalculateMinimumTextRenderingWidth(FontFamily fontFamily, double fontSize, int numberTextLength)
        {
            var cacheKey = $"{fontFamily.Source}-{(int)fontSize}-{numberTextLength}";

            if (_miniRequisiteIntegerTextRenderingWidthCache.ContainsKey(cacheKey))
            {
                return _miniRequisiteIntegerTextRenderingWidthCache[cacheKey];
            }

            double minRequisiteWidth = 0;

            for (int i = 0; i < 10; i++)
            {
                var str = new string((char)('0' + i), numberTextLength);
                var width = GetTextSize(fontFamily, fontSize, str).Width;
                if (width > minRequisiteWidth)
                {
                    minRequisiteWidth = width;
                }
            }

            _miniRequisiteIntegerTextRenderingWidthCache[cacheKey] = minRequisiteWidth;
            return minRequisiteWidth;
        }
        private Dictionary<int, Rect> GetLinenumberPos(string[] lines, ITextRange startRange, ITextRange endRange)
        {
            var offset = 0;
            var lineRects = new Dictionary<int, Rect>(); // 1 - based

            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];

                // Use "offset + line.Length + 1" instead of just "offset" here is to capture the line right above the viewport
                if (offset + line.Length + 1 >= startRange.StartPosition && offset <= endRange.EndPosition)
                {
                    textbox.Document.GetRange(offset, offset + line.Length)
                        .GetRect(PointOptions.ClientCoordinates, out var rect, out _);

                    lineRects[i + 1] = rect;
                }
                else if (offset > endRange.EndPosition)
                {
                    break;
                }

                offset += line.Length + 1; // 1 for line ending: '\r'
            }

            return lineRects;
        }
        public void DoRenderLineNumbers(Dictionary<int, Rect> lineNumberTextRenderingPositions, double minLineNumberTextRenderingWidth)
        {
            var padding = tcb.FontSize / 2;
            var lineNumberPadding = new Thickness(padding, 2, padding + 2, 2);
            var lineNumberTextBlockHeight = tcb.GetSingleLineHeight() + tcb.Padding.Top + lineNumberPadding.Top;
            var numOfReusableLineNumberBlocks = RenderedLineNumbers.Count;

            foreach (var (lineNumber, rect) in lineNumberTextRenderingPositions)
            {
                var margin = new Thickness(lineNumberPadding.Left,
                rect.Top + lineNumberPadding.Top + tcb.Padding.Top,
                lineNumberPadding.Right,
                lineNumberPadding.Bottom);

                if (numOfReusableLineNumberBlocks > 0)
                {
                    var index = numOfReusableLineNumberBlocks - 1;
                    var ln = RenderedLineNumbers[index];
                    ln.Text = lineNumber.ToString();
                    ln.Margin = margin;
                    ln.Height = lineNumberTextBlockHeight;
                    ln.Width = minLineNumberTextRenderingWidth;
                    ln.Visibility = Visibility.Visible;
                    ln.Foreground = new SolidColorBrush(tcb.LineNumberForeground);

                    numOfReusableLineNumberBlocks--;
                }
                else
                {
                    var lineNumberBlock = new TextBlock()
                    {
                        Text = lineNumber.ToString(),
                        Height = lineNumberTextBlockHeight,
                        Width = minLineNumberTextRenderingWidth,
                        Margin = margin,
                        TextAlignment = TextAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalTextAlignment = TextAlignment.Right,
                        Foreground = new SolidColorBrush(tcb.LineNumberForeground)
                    };

                    tcb.LineNumberCanvas.Children.Add(lineNumberBlock);
                    RenderedLineNumbers.Add(lineNumberBlock);
                }
            }

            // Hide all unused rendered linenumber blocks to avoid collosion
            for (int i = 0; i < numOfReusableLineNumberBlocks; i++)
            {
                RenderedLineNumbers[i].Visibility = Visibility.Collapsed;
            }
            tcb.LineNumberGrid.Width = lineNumberPadding.Left + minLineNumberTextRenderingWidth + lineNumberPadding.Right;
        }
    }

    public static class ExtensionClass
    {
        //Extension method to append an element to the stringarray
        public static T[] Append<T>(this T[] array, T item)
        {
            List<T> list = new List<T>(array)
            {
                item
            };

            return list.ToArray();
        }
    }
    public static class ScrollViewerExtensions
    {
        public static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scrollViewer,
            UIElement target,
            Axis axis)
        {
            return scrollViewer.StartExpressionAnimation(target, sourceAxis: axis, targetAxis: axis);
        }

        public static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scrollViewer,
            UIElement target,
            Axis sourceAxis,
            Axis targetAxis)
        {
            try
            {
                CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

                ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{nameof(scrollViewer)}.{nameof(UIElement.Translation)}.{sourceAxis}");
                animation.SetReferenceParameter(nameof(scrollViewer), scrollSet);

                Visual visual = ElementCompositionPreview.GetElementVisual(target);
                visual.StartAnimation($"{nameof(Visual.Offset)}.{targetAxis}", animation);

                return animation;
            }
            catch
            {
                //Prevent crashing
                return null;
            }
        }
    }
}
