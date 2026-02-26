#nullable enable
using System.Collections.Generic;

namespace Perlang;

public record SwitchBranch(List<Expr> Conditions, Stmt.Block Statements);
