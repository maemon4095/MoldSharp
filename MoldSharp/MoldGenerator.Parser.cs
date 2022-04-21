using IncrementalSourceGeneratorSupplement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MoldSharp;

public sealed partial class MoldGenerator
{
    struct GenerationOptions
    {
        public GenerationOptions()
        {
            this.ContextVariable = "context";
            this.ContextSource = "this";
            this.ExportMethod = "ToString";
            this.TransformTextMethod = "TransformText";
            this.TransformTextMethodParams = string.Empty;
            this.ReturnTextType = "string";
            this.TransformTextAccessibility = "public";
        }

        public string ContextVariable { get; set; }
        public bool ExternalContext => this.ContextSource is null;
        public string? ContextSource { get; set; }
        public string ExportMethod { get; set; }
        public string TransformTextMethod { get; set; }
        public string TransformTextMethodParams { get; set; }
        public string ReturnTextType { get; set; }
        public string TransformTextAccessibility { get; set; }
    }
    partial struct Parser
    {
        enum Token
        {
            BlockScriptOpen,
            BlockScriptClose,
            LineScriptOpen,
            ExpressionOpen,
            ExpressionClose,
            LineEnd,
            Quotation,
            DoubleQuotation,
            Escape,
        }

        readonly ref struct Context
        {
            public Context(ReadOnlySpan<char> source, GenerationOptions options)
            {
                this.Source = source;
                this.Root = new IndentedWriter("    ");
                this.TransformText = new IndentedWriter("    ");
                this.Options = options;
            }

            public ReadOnlySpan<char> Source { get; }
            public IndentedWriter Root { get; }
            public IndentedWriter TransformText { get; }
            public GenerationOptions Options { get; }
        }

        static IEnumerable<INamedTypeSymbol> Containings(ITypeSymbol symbol)
        {
            if (symbol.ContainingType is null) yield break;
            foreach (var containing in Containings(symbol.ContainingType)) yield return containing;
            yield return symbol.ContainingType;
        }


        static string? Escape(char chara) => SyntaxFactory.Literal(chara).ToString();

        readonly string TypeDecl;
        readonly string? FullNamespace;
        readonly INamedTypeSymbol Symbol;
        readonly GenerationOptions PreOptions;

        public void Parse(ReadOnlySpan<char> source)
        {
            var fullNamespace = this.FullNamespace;
            var typeDecl = this.TypeDecl;
            var context = ProcessInitialDirective(source, this.PreOptions);
            var options = context.Options;

            if (fullNamespace is not null)
            {
                context.Root["namespace "][fullNamespace].Line()
                            ['{'].Indent(1);
            }

            foreach (var containing in Containings(this.Symbol))
            {
                context.Root["partial "][containing.ToDisplayString(FormatTypeDecl)].Line()
                            ['{'].Line().Indent(1);
            }

            context.Root["partial "][typeDecl].Line()
                        ['{'].Line().Indent(1);

            context.TransformText[options.TransformTextAccessibility][' '][options.ReturnTextType][options.TransformTextMethod]['('][options.TransformTextMethodParams][')'].Line()
                                 ['{'].Line().Indent(1);
            if (!context.Options.ExternalContext)
            {
                context.TransformText["var "][options.ContextVariable][" = "][options.ContextSource][".GetTransformContext();"].Line();
            }

            WriteTransformBody(context);

            context.TransformText.Line()
                                 ["return "][options.ContextVariable]['.'][options.ExportMethod]["();"].Line().Indent(-1)
                                 ['}'].Line();

            context.Root[context.TransformText].End();
            context.Root.Indent(-1)['}'].Line();

            foreach (var _ in Containings(this.Symbol))
            {
                context.Root.Indent(-1)['}'].Line();
            }

            if (fullNamespace is not null) context.Root.Indent(-1)['}'].Line();
        }

        static Context ProcessInitialDirective(ReadOnlySpan<char> source, GenerationOptions options)
        {
            var opt = DirectiveParser.InitialParse(ref source, options);
            return new(source, opt);
        }

        static void WriteTransformBody(Context context)
        {

        }
    }


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
                    case ';' or '\r' or '\n':
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
    }
}