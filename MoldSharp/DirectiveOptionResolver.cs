namespace MoldSharp;

struct DirectiveOptionResolver
{
    public DirectiveOption Resolve(ReadOnlySpan<char> key)
    {

    }
}

delegate void DirectiveOption(ref GenerationOptions options, ReadOnlySpan<char> values);