using System.Collections.Generic;

namespace Perlang.Parser
{
    public class ScanError
    {
        public int Line { get; set; }
        public string Message { get; set; }
    }

    public class ScanErrors : List<ScanError>
    {
        public bool Empty() => Count == 0;
    }
}