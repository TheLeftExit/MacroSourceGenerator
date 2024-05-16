using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

MSBuildLocator.RegisterDefaults();
Main();

void Main()
{
    var root = ProjectRootElement.Create();
    root.AddImport(@"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props");
    root.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");
    root.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");
    root.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");
    var project = new Project(root);
    var result = $"{project.GetPropertyValue("VC_IncludePath")};{project.GetPropertyValue("WindowsSDK_IncludePath")}";
    var targetPath = @"..\..\..\..\IncludePaths.txt";
    File.WriteAllText(targetPath, result);
}