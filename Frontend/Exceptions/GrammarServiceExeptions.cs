using System;

namespace ClearText.Exceptions;

public sealed class GrammarServiceNotReadyException : Exception
{
    public GrammarServiceNotReadyException()
        : base("Grammar service is still starting.") { }
}

public sealed class GrammarServiceUnavailableException : Exception
{
    public GrammarServiceUnavailableException(Exception inner)
        : base("Grammar service failed.", inner) { }
}
