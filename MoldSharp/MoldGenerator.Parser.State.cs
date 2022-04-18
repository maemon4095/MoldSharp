using C4N.Collections.Sequence;
using System.Collections;

namespace MoldSharp;

public sealed partial class MoldGenerator
{
    partial struct Parser
    {
        readonly unsafe struct State
        {
            public static State Initial { get; } = new(&Parser.Initial);
            public static State PlainText { get; } = new(&Parser.PlainText);
            public static State BlockScript { get; } = new(&Parser.BlockScript);
            public static State LineScript { get; } = new(&Parser.LineScript);
            public static State Expression { get; } = new(&Parser.Expression);
            public static State Directive { get; } = new(&Parser.Directive);

            public State(delegate*<Context, State> function)
            {
                this.function = function;
            }

            readonly delegate*<Context, State> function;

            public bool Terminal => this.function is null;

            public State Transition(Context context) => this.function(context);
        }
        static State Initial(Context context)
        {
            var result = context.Source.Read(2);
        }

        static State PlainText(Context context)
        {
            context.TransformText["__context.AppendLiteral(\""].End();
            var next = State.Initial;
            var tokenizer = new PlainTextTokenizer();

            do
            {
                var result = context.Source.Read(2);
                var buffer = result.Buffer;
                var count = 0;
                var escape = false;
                var readPosition = buffer.Tail;

                foreach (var segment in buffer.EnumerateSegment())
                {
                    foreach (var chara in segment.Span)
                    {
                        count++;
                        if (escape)
                        {
                            escape = false;
                            continue;
                        }
                        if (tokenizer.Transition(chara)) continue;
                        var state = tokenizer.State;
                        tokenizer.Reset();

                        if (!state.Accept) continue;

                        switch (state.Associated)
                        {
                            case Token.BlockScriptOpen:
                                next = State.BlockScript;

                                goto END_PlainText;
                            case Token.LineScriptOpen:
                                break;
                            case Token.ExpressionOpen:
                                break;
                            case Token.DirectiveOpen:
                                break;
                            case Token.Escape:
                                break;
                        }
                    }
                }
                context.Source.Advance(readPosition);
            }
            while (true);

            END_PlainText:

            context.TransformText["\");"].Line();
            return next;
        }

        static State BlockScript(Context context)
        {

        }

        static State LineScript(Context context)
        {

        }

        static State Expression(Context context)
        {

        }

        static State Directive(Context context)
        {

        }
    }
}