#nullable enable
using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// A Perlang class, implemented in C++ or Perlang.
    /// </summary>
    public interface IPerlangClass
    {
        public string Name { get; }
        public List<Stmt.Function> Methods { get; }
        public List<Stmt.Field> Fields { get; }
    }
}
