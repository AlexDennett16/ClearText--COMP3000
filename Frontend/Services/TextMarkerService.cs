using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using ClearText.DataObjects;

namespace ClearText.Services
{
    public class TextMarkerService(TextDocument document) : IBackgroundRenderer
    {
        private readonly TextSegmentCollection<TextMarker> _markers = new(document);

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid)
                return;

            foreach (var marker in _markers)
            {
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var pen = new Pen(new SolidColorBrush(marker.Color), 1.5);

                    var start = rect.BottomLeft;
                    var end = rect.BottomRight;

                    var geometry = CreateWavyLine(start, end, 3);
                    drawingContext.DrawGeometry(null, pen, geometry);
                }
            }
        }

        public KnownLayer Layer => KnownLayer.Selection;

        private static StreamGeometry CreateWavyLine(Point start, Point end, double amplitude)
        {
            var geometry = new StreamGeometry();

            using var ctx = geometry.Open();
            ctx.BeginFigure(start, false);

            var x = start.X;
            var up = true;

            while (x < end.X)
            {
                x += 4;
                var y = start.Y + (up ? -amplitude : amplitude);
                ctx.LineTo(new Point(x, y));
                up = !up;
            }

            ctx.EndFigure(false);

            return geometry;
        }

        private void AddMarker(int startOffset, int length, Color color, ClearTextError error)
        {
            var marker = new TextMarker(startOffset, length, color, error);
            _markers.Add(marker);
        }

        internal void ClearMarkers()
        {
            _markers.Clear();
        }

        internal void Remove(TextMarker marker)
        {
            _markers.Remove(marker);
        }


        internal void LoadSquigglies(string editorText, IReadOnlyList<ClearTextError> errors)
        {
            var start = 0;
            foreach (var error in errors)
            {
                var search = error.Token;
                while (true)
                {
                    var index = editorText.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                    if (index == -1)
                        break;

                    AddMarker(index, search.Length, Colors.Red, error);
                    start = index + search.Length;
                }
            }
        }

        internal TextMarker? GetMarkerAtOffset(int offset)
        {
            return _markers.FirstOrDefault(m =>
                m.StartOffset <= offset &&
                offset <= m.EndOffset);
        }


        internal class TextMarker : TextSegment
        {
            public Color Color { get; }
            public ClearTextError Error { get; }

            public TextMarker(int start, int length, Color color, ClearTextError error)
            {
                StartOffset = start;
                Length = length;
                Color = color;
                Error = error;
            }

        }
    }
}
