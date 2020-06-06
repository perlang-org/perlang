namespace Perlang
{
    /// <summary>
    /// "Null-object". This object is used in cases where we need a Type that corresponds to null values. Since
    /// typeof(null) is invalid, we need a fake construct we can use instead.
    /// </summary>
    public abstract class NullObject
    {
    }
}
