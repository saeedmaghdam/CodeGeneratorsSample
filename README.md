# C# Source Generator Sample

This repository demonstrates the use of C# Source Generators (introduced in .NET 5) with a simple example implementation using the Incremental Generator model introduced in .NET 6.

## Project Structure

- **MyCodeGenerator**: A .NET Standard 2.0 library that contains the source generator implementation
- **MyApp**: A .NET 8.0 console application that uses the source generator

## What This Sample Demonstrates

This sample shows how to:

1. Create a source generator using the Roslyn Compiler API
2. Generate C# code at compile time
3. Implement attribute-based code generation
4. Reference and use source generators in a client application

## Source Generator Details

The `HelloWorldGenerator` implements `IIncrementalGenerator` (the recommended approach since .NET 6) and provides the following functionality:

1. Generates two marker attributes:
   - `AutoExtendAttribute`: Used to mark classes for extension
   - `ExtendedAttribute`: Applied to generated code

2. Generates a `HelloWorld` class with a `Message` property that returns "Hello from Source Generator!"

3. Finds all classes marked with `[AutoExtend]` and generates partial class extensions for them

## How It Works

1. The generator identifies classes with the `[AutoExtend]` attribute
2. For each matching class, it generates a corresponding partial class with the `[Extended]` attribute
3. The generated code is automatically compiled into the assembly at build time
4. The generated classes can be used like any other class in your application

## How to Run

1. Build the solution:
   ```
   dotnet build
   ```

2. Run the MyApp project:
   ```
   dotnet run --project MyApp/MyApp.csproj
   ```

## Code Examples

### Source Generator Implementation

```csharp
[Generator]
public class HelloWorldGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute definitions
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("AutoExtendAttribute.g.cs", SourceText.From(@"
                using System;
                [AttributeUsage(AttributeTargets.Class)]
                public class AutoExtendAttribute : Attribute { }
            ", Encoding.UTF8));
            
            // Additional attribute definition and class generation...
        });
        
        // Find classes with [AutoExtend] attribute and generate code
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithAttributes(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);
            
        // Generate partial class with [Extended] attribute
        context.RegisterSourceOutput(classDeclarations, (spc, classSymbol) => {
            // Code to generate the partial class...
        });
    }
}
```

### Using Generated Code

```csharp
// Using the generated HelloWorld class
var hello = new HelloWorld();
Console.WriteLine(hello.Message); // Outputs: "Hello from Source Generator!"

// Using a class marked for extension
[AutoExtend]
public partial class MyService { }
```

## Key Features of Source Generators

- Compile-time code generation
- No runtime reflection needed
- Better performance than runtime code generation
- Fully integrated with the compilation process
- IntelliSense support for generated code

## Requirements

- .NET SDK 8.0 or later
- Visual Studio 2022 or Visual Studio Code with C# extension

## Learn More About Source Generators

- [Source Generators Overview](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) - Official Microsoft documentation
- [Incremental Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md) - Detailed implementation guide
- [Source Generator Updates: Incremental Generators](https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/) - Andrew Lock's article on incremental generators
- [C# Source Generators Collection](https://github.com/amis92/csharp-source-generators) - A curated list of community source generators
- [Incremental Roslyn Source Generators In .NET 6: Code Sharing Of The Future – Part 1](https://www.thinktecture.com/en/net/roslyn-source-generators-introduction/)

## Performance Benefits of Incremental Generators

The `IIncrementalGenerator` interface (introduced in .NET 6) provides significant performance improvements over the original `ISourceGenerator`:

- Uses caching and memoization to avoid redundant work
- Creates a pipeline of transformations rather than processing all syntax nodes
- Significantly improves performance in large solutions
- Reduces IDE lag when editing code

---

Feel free to explore and modify this sample to learn more about C# source generators and their capabilities!
