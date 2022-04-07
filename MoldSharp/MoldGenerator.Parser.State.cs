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
            var buffer = result.Buffer;
            var tokenizer = new InitialTokenizer();
            var (state, _) = buffer.StartsWith<InitialTokenizer, char, Token>(tokenizer);

            if (!state.Accept) return State.PlainText;

            context.Source.Advance(2);
            return state.Associated switch
            {
                Token.BlockScriptOpen => State.BlockScript,
                Token.LineScriptOpen => State.LineScript,
                Token.ExpressionOpen => State.Expression,
                Token.DirectiveOpen => State.Directive,
                _ => default,
            };
        }

        static State PlainText(Context context)
        {
            context.TransformText["__context.AppendLiteral(\""].End();
            var result = context.Source.Read(2);
            var buffer = result.Buffer;
            var next = State.Initial;

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