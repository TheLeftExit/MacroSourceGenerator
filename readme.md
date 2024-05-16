# MacroSourceGenerator

**IncludePathResolver** uses MSBuild to retrieve the values of `$(VC_IncludePath)`, `$(WindowsSDK_IncludePath)` and dump it to a shared text file. This has to run in a separate .NET Framework process because `Microsoft.Build` can only locate and use Visual Studio's MSBuild in the .NET Framework version of the package.

**HeaderFileParser** emulates `framework.h` from Visual Studio's "Windows Desktop Application" project template and parses the resulting header file tree using simplified pre-processor grammar/math. It then dumps the results to a shared text file.

**MacroSourceGenerator** generates C# bindings from the parsed macro list.

---

This project exists because the metadata in [Win32Metadata](https://github.com/microsoft/win32metadata) is far too incomplete, and the project itself seems too complicated to get into. Meanwhile, [CsWin32](https://github.com/microsoft/CsWin32) (which uses this metadata) is incredibly inflexible in its code generation options (target namespaces/classes, parameter types). I don't see these projects meeting my requirements in the foreseeable future.