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

        internal List<TextMarker> GetMarkers()
        {
            return _markers.ToList();
        }

        internal void LoadSquigglies(string editorText, IReadOnlyList<ClearTextError> errors)
        {
            if (errors.Count == 0)
                return;

            // Split the editor text into tokens exactly like Python did
            // (Python uses simple whitespace/punctuation tokenization)
            var tokens = new List<string>();
            var tokenOffsets = new List<int>();

            var i = 0;

            while (i < editorText.Length)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(editorText[i]))
                {
                    i++;
                    continue;
                }

                var start = i;

                // Consume letters/numbers
                while (i < editorText.Length && char.IsLetterOrDigit(editorText[i]))
                    i++;

                // If we captured a word
                if (start != i)
                {
                    var word = editorText[start..i];
                    tokens.Add(word);
                    tokenOffsets.Add(start);
                    continue;
                }

                // Otherwise it's punctuation
                tokens.Add(editorText[i].ToString());
                tokenOffsets.Add(i);
                i++;
            }

            // Now apply markers using Python's token index
            foreach (var error in errors)
            {
                if (error.Index < 0 || error.Index >= tokens.Count)
                    continue;

                var charOffset = tokenOffsets[error.Index];
                var length = tokens[error.Index].Length;

                AddMarker(charOffset, length, Colors.Red, error);
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
