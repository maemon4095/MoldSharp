using IncrementalSourceGeneratorSupplement;

namespace MoldSharp;

public sealed partial class MoldGenerator
{
    struct TemplateParser
    {
        enum State
        {
            Initial,
            PlainText,
            MaybeScriptOpen,
            MaybeBlockScriptClose,
            MaybeExpressionClose,
            MaybeLineEnd,
            Directive,
            LineScript,
            BlockScript,
            Expression,
            CharLiteral,
            CharLiteralEscape,
            StringLiteral,
            StringLiteralEscape,
        }

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


        public TemplateParser(IndentedWriter rootWriter, string typeIdentifier, string? fullNamespace)
        {
            this.rootWriter = rootWriter;
            this.state = State.Initial;
            this.prevState = State.Initial;
            this.TypeIdentifier = typeIdentifier;
            this.FullNamespace = fullNamespace;
            var transformTextWriter = this.transformTextWriter = new IndentedWriter(rootWriter.IndentString);


            if (fullNamespace is not null)
            {
                rootWriter["namespace "][fullNamespace].Line()
                          ['{'].Indent(1);
            }

            transformTextWriter["public string TransformText()"].Line()
                               ['{'].Line().Indent(1)
                               ["var __context = this.GetTransformContext();"].Line();
        }

        readonly IndentedWriter rootWriter;
        readonly IndentedWriter transformTextWriter;
        readonly string TypeIdentifier;
        readonly string? FullNamespace;
        State state, prevState;

        public void Parse(ReadOnlySpan<char> span)
        {
            var writer = this.transformTextWriter;
            if (span.IsEmpty)
            {
                writer.Line()
                      ["return "]["__context.ToString();"].Line().Indent(-1)
                      ['}'].Line();

                var rootWriter = this.rootWriter;
                rootWriter["partial "][this.TypeIdentifier].Line()
                          ['{'].Line().Indent(1);

                rootWriter[writer].End();

                rootWriter.Indent(-1)['}'].Line();

                if (this.FullNamespace is not null)
                {
                    rootWriter.Indent(-1)['}'].Line();
                }
                return;
            }

            var state = this.state;
            var prevState = this.prevState;
            foreach (var chara in span)
            {
                switch (state)
                {
                    case State.Initial:
                        switch (chara)
                        {
                            case '/':
                                prevState = State.Initial;
                                state = State.MaybeScriptOpen;
                                break;
                            default:
                                writer["__context.AppendLiteral(\""].End();
                                state = State.PlainText;
                                goto STATE_PlainText;
                        }
                        break;
                    case State.PlainText:
                        STATE_PlainText:
                        switch (chara)
                        {
                            case '/':
                                prevState = state;
                                state = State.MaybeScriptOpen;
                                break;
                            default:
                                if (Escape(chara) is string str) writer[str].End();
                                else writer[chara].End();
                                break;
                        }
                        break;
                    case State.MaybeScriptOpen:
                        switch (chara)
                        {
                            case '@':
                                if (prevState == State.PlainText) writer["\");"].Line();
                                state = State.Directive;
                                break;
                            case '#':
                                if (prevState == State.PlainText) writer["\");"].Line();
                                state = State.LineScript;
                                break;
                            case ':':
                                if (prevState == State.PlainText) writer["\");"].Line();
                                state = State.BlockScript;
                                break;
                            case '$':
                                if (prevState == State.PlainText) writer["\");"].Line();
                                state = State.Expression;
                                writer["__context.Append("].End();
                                break;
                            default:
                                writer['/'].End();
                                state = State.PlainText;
                                goto STATE_PlainText;
                        }
                        break;
                    case State.MaybeBlockScriptClose:
                        switch (chara)
                        {
                            case '/':
                                prevState = State.BlockScript;
                                state = State.Initial;
                                writer.Line();
                                break;
                            default:
                                writer[':'].End();
                                state = State.BlockScript;
                                goto STATE_BlockScript;
                        }
                        break;
                    case State.MaybeExpressionClose:
                        switch (chara)
                        {
                            case '/':
                                prevState = State.Expression;
                                state = State.Initial;
                                writer[");"].Line().End();
                                break;
                            default:
                                state = State.Expression;
                                writer['$'].End();
                                goto STATE_Expression;
                        }
                        break;
                    case State.MaybeLineEnd:
                        switch (chara)
                        {
                            case '\n':
                                writer.Line();
                                state = State.Initial;
                                break;
                            default:
                                state = State.PlainText;
                                goto STATE_PlainText;
                        }
                        break;
                    case State.CharLiteral:
                        switch (chara)
                        {
                            case '\\':
                                state = State.CharLiteralEscape;
                                break;
                            case '\'':
                                state = prevState;
                                break;
                            default:
                                break;
                        }
                        writer[chara].End();
                        break;
                    case State.CharLiteralEscape:
                        writer[chara].End();
                        state = State.CharLiteral;
                        break;
                    case State.StringLiteral:
                        switch (chara)
                        {
                            case '\\':
                                state = State.StringLiteralEscape;
                                break;
                            case '\"':
                                state = prevState;
                                break;
                            default:
                                break;
                        }
                        writer[chara].End();
                        break;
                    case State.StringLiteralEscape:
                        writer[chara].End();
                        state = State.StringLiteral;
                        break;
                    case State.Directive:
                        switch (chara)
                        {
                            case '\r':
                                prevState = State.Directive;
                                state = State.MaybeLineEnd;
                                break;
                            case '\n':
                                prevState = State.Directive;
                                state = State.Initial;
                                writer.Line();
                                break;
                            default:
                                break;
                        }
                        break;
                    case State.LineScript:
                        switch (chara)
                        {
                            case '\r':
                                prevState = State.LineScript;
                                state = State.MaybeLineEnd;
                                break;
                            case '\n':
                                prevState = State.LineScript;
                                state = State.Initial;
                                writer.Line();
                                break;
                            case '\'':
                                prevState = State.LineScript;
                                state = State.CharLiteral;
                                writer[chara].End();
                                break;
                            case '\"':
                                prevState = State.LineScript;
                                state = State.StringLiteral;
                                writer[chara].End();
                                break;
                            default:
                                writer[chara].End();
                                break;
                        }
                        break;
                    case State.BlockScript:
                        STATE_BlockScript:
                        switch (chara)
                        {
                            case '\'':
                                prevState = State.BlockScript;
                                state = State.CharLiteral;
                                writer[chara].End();
                                break;
                            case '\"':
                                prevState = State.BlockScript;
                                state = State.StringLiteral;
                                writer[chara].End();
                                break;
                            case ':':
                                prevState = State.BlockScript;
                                state = State.MaybeBlockScriptClose;
                                break;
                            default:
                                writer[chara].End();
                                break;
                        }
                        break;
                    case State.Expression:
                        STATE_Expression:
                        switch (chara)
                        {
                            case '\'':
                                prevState = State.Expression;
                                state = State.CharLiteral;
                                writer[chara].End();
                                break;
                            case '\"':
                                prevState = State.Expression;
                                state = State.StringLiteral;
                                writer[chara].End();
                                break;
                            case '$':
                                prevState = State.Expression;
                                state = State.MaybeExpressionClose;
                                break;
                            default:
                                writer[chara].End();
                                break;
                        }
                        break;
                }
            }
            this.state = state;
            this.prevState = prevState;
        }
    }
}