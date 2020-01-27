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

        // Convenience method to free consumers from having to construct ScanErrors manually.
        public void Add(in int line, string message)
        {
            Add(new ScanError { Line = line, Message = message });
        }
    }
}