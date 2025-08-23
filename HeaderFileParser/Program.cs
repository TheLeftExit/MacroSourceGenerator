using System.Diagnostics;

Console.WriteLine("Locating the C/C++ build system...");

var vswherePath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe");
var vswhereArgs = @"-products * -latest -prerelease -find **\VC\Auxiliary\Build\vcvarsall.bat";

var vswhereStartInfo = new ProcessStartInfo
{
    FileName = vswherePath,
    Arguments = vswhereArgs,
    RedirectStandardOutput = true,
}; 
var vswhereProcess = Process.Start(vswhereStartInfo);
await vswhereProcess.WaitForExitAsync();
var vcvarsallPath = await vswhereProcess.StandardOutput.ReadLineAsync(); // just the first line

var cmdArgs = $@"/c ""call ""{vcvarsallPath}"" x64 > NUL && SET""";
var cmdStartInfo = new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = cmdArgs,
    RedirectStandardOutput = true
};
var cmdProcess = Process.Start(cmdStartInfo);
//await cmdProcess.WaitForExitAsync();
var cmdOutput = await cmdProcess.StandardOutput.ReadToEndAsync();

var vars = cmdOutput.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x =>
{
    var splitIndex = x.IndexOf('=');
    return (Name: x[..splitIndex], Value: x[(splitIndex + 1)..]);
}).ToDictionary(x => x.Name, x => x.Value);

FileUtils.Init(vars["INCLUDE"]);

var framework =
"""
#include <windows.h>
#include <stdlib.h>
#include <malloc.h>
#include <memory.h> 
#include <tchar.h>
#include <commctrl.h>
""";

Console.WriteLine("Processing header files...");
var processor = new FileProcessor();
processor.Process(framework);
var macros = processor.Macros;
Console.WriteLine($"Processing finished. Macros found: {macros.Length}");

while (true)
{
    Console.Write("Prefix: ");
    var input = Console.ReadLine();

    var result = macros
        .Where(x => x.Name.StartsWith(input))
        .Select(x =>
        $"public const UINT {x.Name.Trim()} = {string.Join(' ', x.ValueTokens).TrimEnd('L')};\r\n");
    var resultString = string.Concat(result);

    Console.WriteLine(resultString);
}



