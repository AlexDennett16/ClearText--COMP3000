using System;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace ClearText.Services
{
    public class TextMarkerService(TextDocument document) : IBackgroundRenderer
    {
        private readonly TextSegmentCollection<TextMarker> _markers = new(document);

        public void AddMarker(int startOffset, int length, Color color)
        {
            var marker = new TextMarker(startOffset, length, color);
            _markers.Add(marker);
        }

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

            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(start, false);

                double x = start.X;
                bool up = true;

                while (x < end.X)
                {
                    x += 4;
                    double y = start.Y + (up ? -amplitude : amplitude);
                    ctx.LineTo(new Point(x, y));
                    up = !up;
                }

                ctx.EndFigure(false);
            }

            return geometry;
        }

        public void ClearMarkers()
        {
            _markers.Clear();
        }

        public void LoadSquigglies(string editorText)
        {

            var search = "banana";
            var start = 0;

            while (true)
            {
                var index = editorText.IndexOf(search, start, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    break;

                AddMarker(index, search.Length, Colors.Red);

                start = index + search.Length;
            }


        }

        private class TextMarker : TextSegment
        {
            public Color Color { get; }

            public TextMarker(int start, int length, Color color)
            {
                StartOffset = start;
                Length = length;
                Color = color;
            }
        }
    }
}