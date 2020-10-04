using System.Reflection;
using System.Runtime.CompilerServices;
using Perlang;

[assembly: AssemblyVersion(CommonConstants.Version)]
[assembly: AssemblyInformationalVersion(CommonConstants.Version + "+" + CommonConstants.GitVersion)]
[assembly: InternalsVisibleTo("Perlang.Tests")]
