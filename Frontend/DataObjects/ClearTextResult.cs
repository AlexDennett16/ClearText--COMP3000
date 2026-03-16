using System.Collections.Generic;

namespace ClearText.DataObjects;

public class ClearTextResult
{
    public required string Text { get; set; }
    public required List<string> Tokens { get; set; }
    public required List<ClearTextError> Errors { get; set; }
}