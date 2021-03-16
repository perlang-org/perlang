using System.Reflection;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Container class for a target object instance and a method.
    ///
    /// The object instance can be null, which is the correct state for representing method calls to static methods.
    /// </summary>
    public class TargetAndMethodContainer
    {
        public object Target { get; }
        public MethodInfo Method { get; }

        public TargetAndMethodContainer(object target, MethodInfo method)
        {
            Target = target;
            Method = method;
        }

        public override string ToString()
        {
            if (Target != null)
            {
                // Slightly inspired by Ruby, where "Integer.method(:to_s)" returns "#<Method: Integer.to_s>"
                return $"#<{Target} {Method}>";
            }
            else
            {
                return Method.ToString();
            }
        }
    }
}
