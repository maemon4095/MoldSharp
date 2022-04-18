using StaticStateMachine.Generator;
namespace MoldSharp;

public sealed partial class MoldGenerator
{
    partial struct Parser
    {
        [StaticStateMachine]
        [Association("/: ", Token.BlockScriptOpen)]
        [Association("/# ", Token.LineScriptOpen)]
        [Association("/$ ", Token.ExpressionOpen)]
        [Association("\\", Token.Escape)]
        partial struct PlainTextTokenizer { }

        [StaticStateMachine]
        [Association("/: ", Token.BlockScriptOpen)]
        [Association("/# ", Token.LineScriptOpen)]
        [Association("/$ ", Token.ExpressionOpen)]
        partial struct InitialTokenizer { }

        [StaticStateMachine]
        [Association(" :/", Token.BlockScriptClose)]
        [Association(" $/", Token.ExpressionOpen)]
        [Association("\r", Token.LineEnd)]
        [Association("\r\n", Token.LineEnd)]
        [Association("\n", Token.LineEnd)]
        [Association("'", Token.Quotation)]
        [Association("\"", Token.DoubleQuotation)]
        [Association("\\", Token.Escape)]
        partial struct ScriptTokenizer { }

        [StaticStateMachine]
        [Association("context-variable", DirectiveToken.ContextVariable)]
        [Association("context-source", DirectiveToken.ContextSource)]
        [Association("method-signeture", DirectiveToken.MethodSigneture)]
        [Association("export-method", DirectiveToken.ExportMethod)]
        [Association("external-context", DirectiveToken.ExternalContext)]
        [Association("import", DirectiveToken.Import)]
        partial struct DirectiveTokenizer { }
    }
}