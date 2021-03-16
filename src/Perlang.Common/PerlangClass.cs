#nullable enable
using System.Collections.Generic;

namespace Perlang
{
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
