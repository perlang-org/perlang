using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Perlang.Interpreter;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("printf")]
    public class PrintfCallable : NativeCallable
    {
        public override object Call(IInterpreter interpreter, List<object> arguments)
        {
            int nextArgument = 1;

            switch (arguments.Count)
            {
                case 0:
                    // No arguments; we could either return or throw an exception in this case.
                    break;

                default:
                    if (!(arguments[0] is string formatString))
                    {
                        throw new RuntimeError(null,
                            $"First parameter must be string, not {arguments[0].GetType().Name}");
                    }

                    // FIXME: Replace with more efficient implementation using a char array; this one is not optimized
                    // for speed in any way.
                    var sb = new StringBuilder();

                    for (int i = 0; i < formatString.Length; i++)
                    {
                        var c = formatString[i];
                        var d = (i < formatString.Length - 1) ? formatString[i + 1] : '\0';

                        if (c == '%')
                        {
                            switch (d)
                            {
                                case '%':
                                    sb.Append('%');
                                    i++;
                                    break;

                                case 'd':
                                    sb.Append(((double) arguments[nextArgument]).ToString(CultureInfo
                                        .InvariantCulture));
                                    i++;
                                    break;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    interpreter.StandardOutputHandler(sb.ToString());

                    break;
            }

            return null;
        }

        public override int Arity()
        {
            return -1;
        }
    }
}
