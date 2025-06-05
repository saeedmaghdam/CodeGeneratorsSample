using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MyCodeGenerator
{
    [Generator]
    public class HelloWorldGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the marker attributes
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("AutoExtendAttribute.g.cs", SourceText.From(@"
using System;
[AttributeUsage(AttributeTargets.Class)]
public class AutoExtendAttribute : Attribute { }
            ", Encoding.UTF8));

                ctx.AddSource("ExtendedAttribute.g.cs", SourceText.From(@"
using System;
[AttributeUsage(AttributeTargets.Class)]
public class ExtendedAttribute : Attribute { }
            ", Encoding.UTF8));
            });

            // Generate HelloWorld class
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("HelloWorld.g.cs", SourceText.From(@"
namespace Generated
{
    public class HelloWorld
    {
        public string Message => ""Hello from Source Generator!"";
    }
}
            ", Encoding.UTF8));
            });

            // Identify classes with [AutoExtend] attribute
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassWithAttributes(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            // Generate partial class with [Extended] attribute
            context.RegisterSourceOutput(classDeclarations, (spc, classSymbol) =>
            {
                var ns = classSymbol.ContainingNamespace.ToDisplayString();
                var name = classSymbol.Name;
                var source = $@"
namespace {ns}
{{
    [Extended]
    partial class {name} {{
        // This class is extended
        // by the source generator with the [Extended] attribute.

        public void SayHello() {{
            Console.WriteLine(""Hello from the extended class!"");
        }}
    }}
}}
            ";
                spc.AddSource($"{name}_Extended.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static bool IsClassWithAttributes(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Count > 0;
        }

        private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            foreach (var attributeList in classDecl.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                    if (symbol?.ContainingType.Name == "AutoExtendAttribute")
                    {
                        return context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                    }
                }
            }
            return null;
        }
    }
}
