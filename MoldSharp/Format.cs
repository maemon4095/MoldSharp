using Microsoft.CodeAnalysis;

namespace MoldSharp;

static class Format
{
    public static SymbolDisplayFormat TypeDecl { get; } = new SymbolDisplayFormat(
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance
    );

    public static SymbolDisplayFormat FileName { get; } = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
}
