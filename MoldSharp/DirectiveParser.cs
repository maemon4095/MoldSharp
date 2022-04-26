namespace MoldSharp;

struct DirectiveParser
{
    public static GenerationOptions InitialParse(ref ReadOnlySpan<char> source, GenerationOptions options)
    {
        do
        {
            if (!source.StartsWith("/@")) break;
            source = source.Slice(2);
            var key = PopKey(ref source);
            var value = PopValue(ref source);


            if (source.IsEmpty) break;

            source = source.Slice(source.StartsWith("\r\n") ? 2 : 1);
        }
        while (true);
        return options;
    }
    static ReadOnlySpan<char> PopKey(ref ReadOnlySpan<char> source)
    {
        source = source.TrimStart();
        for (var i = 0; i < source.Length; ++i)
        {
            switch (source[i])
            {
                case ';' or ' ' or '\r' or '\n':
                    var r = source.Slice(0, i);
                    source = source.Slice(i);
                    return r;
                default: break;
            }
        }

        var result = source;
        source = ReadOnlySpan<char>.Empty;
        return result;

    }
    static ReadOnlySpan<char> PopValue(ref ReadOnlySpan<char> source)
    {
        source = source.TrimStart();
        for (var i = 0; i < source.Length; ++i)
        {
            switch (source[i])
            {
                case '\r':
                {
                    var r = source.Slice(0, i);
                    source = (source.Length > i + 1 && source[i + 1] == '\n') ? source.Slice(i + 1) : source.Slice(i);
                    return r;
                }
                case ';' or ' ' or '\n':
                {
                    var r = source.Slice(0, i);
                    source = source.Slice(i);
                    return r;
                }
                default: break;
            }
        }

        var result = source;
        source = ReadOnlySpan<char>.Empty;
        return result;
    }
}