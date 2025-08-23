# MacroSourceGenerator

This script:
- Uses `vswhere` to locate `vsvarsall.bat`,
- Uses `vcvarsall.bat` to retrieve the `INCLUDE` variable,
- Emulates `framework.h` from Visual Studio's "Windows Desktop Application" project template, and parses the resulting header file tree using simplified pre-processor grammar/math.  
*You know, I'm something of a compiler writer myself.*

How to use:
- Run and wait for it to initialize,
- Enter the prefix for the required macros (e.g., `WS_`),
- The tool dumps all macros formatted as `public const UINT <...>;` fields. You can change the output format in the source code.

I use this script to generate C# bindings for Win32 macros for my projects. Why reinvent the wheel - because the metadata in [Win32Metadata](https://github.com/microsoft/win32metadata) is far too incomplete, and [CsWin32](https://github.com/microsoft/CsWin32) (which uses this metadata) is incredibly opinionated and inflexible in its code generation options (target namespaces/classes, parameter types, etc.).