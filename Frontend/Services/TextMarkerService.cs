using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using ClearText.BaseTypes;
using ClearText.DataObjects;

namespace ClearText.Services;

// Service responsible for managing the text markers (squigglies) shown in the TextEditorViewModel
public sealed class TextMarkerService(TextDocument document) : BaseService, IBackgroundRenderer
{
    private readonly TextSegmentCollection<TextMarker> _markers = new(document);
    //This layer exists above the text, but below the caret, so squiggles live nestled between
    public KnownLayer Layer => KnownLayer.Selection;

    // Automatically Called by Avalonia whenever editor background needs redrawing, through IBackgroundRenderer interface
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid)
            return;

        foreach (var marker in _markers)
        {
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                //Prep drawing squiggly below text using red colour from marker and given wavy line
                var pen = new Pen(new SolidColorBrush(marker.Color), 1.5);
                var start = rect.BottomLeft;
                var end = rect.BottomRight;
                var geometry = CreateWavyLine(start, end, 3);

                drawingContext.DrawGeometry(null, pen, geometry);
            }
        }
    }

    // Creates a wavy line geometry between the start and end points, with the specified amplitude
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


    internal void ClearMarkers() => _markers.Clear();

    internal TextMarker? GetMarkerAtOffset(int offset)
    {
        return _markers.FirstOrDefault(m =>
            m.StartOffset <= offset &&
            offset <= m.EndOffset);
    }

    private void AddMarker(
        int startOffset,
        int length,
        Color color,
        ClearTextError error)
    {
        _markers.Add(
            new TextMarker(startOffset, length, color, error)
        );
    }


    internal void LoadSquigglies(
        string editorText,
        IReadOnlyList<string> tokens,
        IReadOnlyList<ClearTextError> errors)
    {
        ClearMarkers();

        // Compute start offsets once
        var tokenOffsets = new List<int>();
        var cursor = 0;

        foreach (var token in tokens)
        {
            // Skip whitespace
            while (cursor < editorText.Length &&
                   char.IsWhiteSpace(editorText[cursor]))
            {
                cursor++;
            }

            tokenOffsets.Add(cursor);
            cursor += token.Length;
        }

        foreach (var error in errors)
        {
            if (error.Index < 0 || error.Index >= tokens.Count)
            {
                continue;
            }

            var start = tokenOffsets[error.Index];
            var length = tokens[error.Index].Length;

            AddMarker(start, length, Colors.Red, error);
        }
    }

    internal void Remove(TextMarker marker)
    {
        _markers.Remove(marker);
    }


    internal class TextMarker : TextSegment
    {
        public Color Color { get; }
        public ClearTextError Error { get; }

        public TextMarker(
            int start,
            int length,
            Color color,
            ClearTextError error)
        {
            StartOffset = start;
            Length = length;
            Color = color;
            Error = error;
        }
    }
}