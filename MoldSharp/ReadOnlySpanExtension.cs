using StaticStateMachine;

namespace MoldSharp;

static class ReadOnlySpanExtension
{
    public static bool StartsWith(this ReadOnlySpan<char> span, string str)
    {
        return span.StartsWith(str.AsSpan());
    }
}