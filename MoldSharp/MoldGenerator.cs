using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using C4N.Collections.Sequence;
using IncrementalSourceGeneratorSupplement;

namespace MoldSharp;

[Generator]
public sealed partial class MoldGenerator : IncrementalSourceGeneratorBase<TypeDeclarationSyntax>
{
    static SymbolDisplayFormat FormatTypeDecl { get; } = new SymbolDisplayFormat(
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance);
    static SymbolDisplayFormat FormatFileName { get; } = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    const string Namespace = nameof(MoldSharp);
    const string AttributeName = "MoldAttribute";
    const string AttributeFullName = $"{Namespace}.{AttributeName}";

    protected override ImmutableArray<AttributeData> FilterAttribute(Compilation compilation, ImmutableArray<AttributeData> attributes)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName) ?? throw new NullReferenceException($"{AttributeFullName} was not found.");
        var filtered = attributes.Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol)).ToImmutableArray();
        return filtered.IsEmpty ? default : filtered;
    }

    protected override (string HintName, SourceText Source) ProductInitialSource()
    {
        var source = SourceText.From(new MoldAttributeTemplate().TransformText(), Encoding.UTF8);
        return ($"{AttributeFullName}.g.cs", source);
    }

    protected override (string HintName, SourceText Source) ProductSource(Compilation compilation, ISymbol baseSymbol, ImmutableArray<AttributeData> attributes)
    {
        var symbol = baseSymbol as INamedTypeSymbol ?? throw new NotSupportedException("Unnamed type is not supported");
        var attributeData = attributes.First();

        var sourcePath = attributeData.ApplicationSyntaxReference?.SyntaxTree.FilePath ?? string.Empty;
        var directoryPath = Directory.GetParent(sourcePath).FullName;
        var relativePath = attributeData.ConstructorArguments.First().Value as string;

        var filePath = Path.Combine(directoryPath, relativePath);
        if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

        var writer = new IndentedWriter("    ");
        using var source = new TextSequenceSource(new StreamReader(new FileStream(filePath, FileMode.Open), Encoding.UTF8));

        var parser = new TemplateParser(writer, 
            symbol.ToDisplayString(FormatTypeDecl), 
            symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString());
        do
        {
            var result = source.Read();
            var buffer = result.Buffer;
            foreach (var memory in buffer)
            {
                parser.Parse(memory.Span);
            }
            source.Advance(buffer.Tail);
            if (result.IsCompleted)
            {
                parser.Parse(ReadOnlySpan<char>.Empty);
                break;
            }
        }
        while (true);

        return ($"{symbol.ToDisplayString(FormatFileName)}.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
    }
}