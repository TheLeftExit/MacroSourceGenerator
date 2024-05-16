using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;


[Generator]
public class MacroSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var macros = GetMacroDefinitions().ToArray();
        var iccMacros = macros.Where(x => x.Name.StartsWith("ICC_")).ToArray();

        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine(@"namespace MacroSourceGenerator;");
        sourceBuilder.AppendLine(@"public static class Macros {");
        foreach (var macro in iccMacros)
        {
            sourceBuilder.AppendLine($"public const int {macro.Name} = {macro.Value};");
        }
        sourceBuilder.AppendLine("}");

        // Add the source code to the compilation
        context.AddSource($"MacroSourceGenerator.g.cs", sourceBuilder.ToString());
    }

    public void Initialize(GeneratorInitializationContext context) { }

    private IEnumerable<MacroDefinition> GetMacroDefinitions()
    {
        using var macroDefinitionsRaw = typeof(MacroSourceGenerator).Assembly
            .GetManifestResourceStream("MacroSourceGenerator.MacroDefinitions.txt");
        using var reader = new StreamReader(macroDefinitionsRaw);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if(string.IsNullOrEmpty(line)) continue;
            yield return MacroDefinition.FromParserLine(line);
        }
    }
}

public class MacroDefinition
{
    public string Name { get; }
    public string[] Parameters { get; }
    public string Value { get; }
    public MacroDefinition(string name, string[] parameters, string value)
    {
        Name = name;
        Parameters = parameters;
        Value = value;
    }

    public static MacroDefinition FromParserLine(string line)
    {
        var parts = line.Split('\t');
        var parameters = parts.Length == 1 && parts[0] == "" ? Array.Empty<string>() : parts[1].Split(',');
        return new MacroDefinition(parts[0], parameters, parts[2]);
    }
}