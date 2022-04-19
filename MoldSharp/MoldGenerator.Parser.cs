using C4N.Collections.Sequence;
using IncrementalSourceGeneratorSupplement;
using Microsoft.CodeAnalysis;
using StaticStateMachine;

namespace MoldSharp;

public sealed partial class MoldGenerator
{
    struct GenerationOptions
    {
        public GenerationOptions()
        {
            this.ContextVariable = "context";
            this.ExternalContext = false;
            this.ContextSource = "this";
            this.ExportMethod = "ToString";
        }

        public string ContextVariable { get; set; }
        public bool? ExternalContext { get; set; }
        public string? ContextSource { get; set; }
        public string ExportMethod { get; set; }
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

        public void Parse(ReadOnlySpan<char> source)
        {
            var fullNamespace = this.FullNamespace;
            var typeDecl = this.TypeDecl;
            var context = ProcessInitialDirective(source);

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

            context.TransformText["public string TransformText()"].Line()
                                 ['{'].Line().Indent(1);
            context.TransformText["var "][context.ContextVariable][" = this.GetTransformContext();"].Line();

            WriteTransformBody(context);

            context.TransformText.Line()
                                 ["return "][context.ContextVariable]['.'][context.ExportMethod]["();"].Line().Indent(-1)
                                 ['}'].Line();

            context.Root[context.TransformText].End();
            context.Root.Indent(-1)['}'].Line();

            foreach (var _ in Containings(this.Symbol))
            {
                context.Root.Indent(-1)['}'].Line();
            }

            if (fullNamespace is not null) context.Root.Indent(-1)['}'].Line();
        }

        static Context ProcessInitialDirective(ReadOnlySpan<char> source)
        {
            var options = DirectiveParser.InitialParse(ref source);
            return new(source, options);
        }

        static void WriteTransformBody(Context context)
        {

        }
    }


    struct DirectiveParser
    {
        public static GenerationOptions InitialParse(ref ReadOnlySpan<char> source)
        {
            
        }
        
    }
}