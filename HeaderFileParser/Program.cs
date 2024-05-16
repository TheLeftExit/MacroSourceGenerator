var framework =
"""
#include <windows.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>
#include <commctrl.h>
""";

var processor = new FileProcessor();
processor.Process(framework);
var macros = processor.Macros;

var targetPath = @"..\..\..\..\MacroDefinitions.txt";
using(var sw = File.CreateText(targetPath))
{
    foreach(var macro in macros)
    {
        // {Name} \t {Comma-separated parameters} \t {Value}
        sw.WriteLine($"{macro.Name}\t{string.Join(',', macro.Parameters ?? Enumerable.Empty<string>())}\t{string.Concat(macro.ValueTokens)}");
    }
}


var macrosWithTabs = macros.Where(x => x.ValueTokens.Contains("\t")).ToList();

;