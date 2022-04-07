using C4N.Collections.Sequence;
using IncrementalSourceGeneratorSupplement;
using StaticStateMachine.Generator;
namespace MoldSharp;

public sealed partial class MoldGenerator
{
    partial struct DirectiveParser
    {
        enum Token
        {
            Import, Static
        }

        [StaticStateMachine]
        [Association("import", Token.Import)]
        [Association("static", Token.Static)]
        partial struct TokenResolver
        {

        }
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
            DirectiveOpen,
            LineEnd,
        }

        readonly struct Context
        {
            public Context(TextSequenceSource source)
            {
                this.Source = source;
                this.Root = new IndentedWriter("    ");
                this.TransformText = new IndentedWriter("    ");
            }

            public TextSequenceSource Source { get; }
            public IndentedWriter Root { get; }
            public IndentedWriter TransformText { get; }
        }

        [StaticStateMachine]
        [Association("/:", Token.BlockScriptOpen)]
        [Association("/#", Token.LineScriptOpen)]
        [Association("/$", Token.ExpressionOpen)]
        [Association("/@", Token.DirectiveOpen)]
        partial struct InitialTokenizer
        { }

        [StaticStateMachine]
        [Association(":/", Token.BlockScriptClose)]
        [Association("$/", Token.ExpressionOpen)]
        [Association("\r", Token.LineEnd)]
        [Association("\r\n", Token.LineEnd)]
        [Association("\n", Token.LineEnd)]
        partial struct ScriptTokenizer
        { }
        static string? Escape(char chara) => chara switch
        {
            '\'' => @"\'",
            '\"' => @"\""",
            '\\' => @"\\",
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\f' => @"\f",
            '\n' => @"\n",
            '\r' => @"\r",
            '\t' => @"\t",
            '\v' => @"\v",
            _ => null,
        };

        readonly string TypeDecl;
        readonly string? FullNamespace;

        public void Parse(TextSequenceSource source)
        {
            var fullNamespace = this.FullNamespace;
            var typeDecl = this.TypeDecl;
            var context = new Context(source);

            if (fullNamespace is not null)
            {
                context.Root["namespace "][fullNamespace].Line()
                            ['{'].Indent(1);
            }

            context.Root["partial "][typeDecl].Line()
                        ['{'].Line().Indent(1);

            context.TransformText["public string TransformText()"].Line()
                                 ['{'].Line().Indent(1)
                                 ["var __context = this.GetTransformContext();"].Line();
            var state = State.Initial;
            while (!state.Terminal) state = state.Transition(context);

            context.TransformText.Line()
                                 ["return "]["__context.ToString();"].Line().Indent(-1)
                                 ['}'].Line();

            context.Root[context.TransformText].End();
            context.Root.Indent(-1)['}'].Line();

            if (fullNamespace is not null) context.Root.Indent(-1)['}'].Line();
        }
    }
}