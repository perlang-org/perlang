#nullable enable
using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// A Perlang class, implemented in C++ or Perlang.
    /// </summary>
    public class PerlangClass
    {
        public string Name { get; }
        public List<Stmt.Function> Methods { get; }

        public PerlangClass(string name, List<Stmt.Function> methods)
        {
            Name = name;
            Methods = methods;
        }

        public override string ToString() =>
            Name;
    }
}
