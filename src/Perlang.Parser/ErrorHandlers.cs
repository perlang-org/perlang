namespace Perlang.Parser;

public delegate void ParseErrorHandler(ParseError parseError);
public delegate void ScanErrorHandler(ScanError scanError);
public delegate bool CompilerWarningHandler(CompilerWarning compilerWarning);