using System.Collections.Generic;

namespace ClearText.DataObjects;

public class ClearTextError
{
    public required string Type { get; set; }
    public required string Token { get; set; }
    public int Index { get; set; }
    public List<string>? Suggestions { get; set; } = null;
}