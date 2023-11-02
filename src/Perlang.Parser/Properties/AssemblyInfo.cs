using System.Reflection;
using System.Runtime.CompilerServices;
using Perlang;

[assembly: AssemblyVersion(CommonConstants.Version)]
[assembly: AssemblyInformationalVersion(CommonConstants.InformationalVersion)]

// This is a bit of a code smell, but: we want to expose these internal types to the Perlang interpreter and compiler,
// but not necessarily make them part of the "public Perlang API" which could potentially be used by a JetBrains Rider
// extension, VS Code language server or similar. Hopefully, this will all become moot since we become self-hosting fast
// enough but time will tell. :-)
[assembly: InternalsVisibleTo("Perlang.Interpreter")]

[assembly: InternalsVisibleTo("Perlang.Tests")]
