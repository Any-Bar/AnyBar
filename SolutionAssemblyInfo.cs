using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("Debug build, https://github.com/Any-Bar/AnyBar")]
#else
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyDescription("Release build, https://github.com/Any-Bar/AnyBar")]
#endif

[assembly: AssemblyCompany("AnyBar Team")]
[assembly: AssemblyProduct("AnyBar")]
[assembly: AssemblyCopyright("The MIT License (MIT)")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: SupportedOSPlatform("windows10.0.19041.0")]
