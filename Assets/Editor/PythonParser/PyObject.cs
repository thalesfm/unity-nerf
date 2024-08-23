using System;

namespace PythonParser
{
    public abstract class PyObject
    {
        public virtual T GetValue<T>() =>
            throw new InvalidOperationException($"{GetType()}.Name doesn't support this operation.");
    }

    public abstract class PyLiteral<T> : PyObject
    {
        public readonly T Value;

        protected PyLiteral(T value)
        {
            Value = value;
        }
        
        // public override T GetValue<T>() => (T) Convert.ChangeType(Value, typeof(T));
    }

    public class PyBool : PyLiteral<bool>
    {
        public PyBool(bool value) : base(value) { }
    }

    // Omitted:
    // public class PythonComplex ...

    public class PyInt : PyLiteral<long>
    {
        public PyInt(long value) : base(value) { }
    }

    public class PyFloat : PyLiteral<double>
    {
        public PyFloat(double value) : base(value) { }
    }

    public class PyNone : PyLiteral<object>
    {
        public PyNone() : base(null) { }
    }

    public class PyString : PyLiteral<string>
    {
        public PyString(string value) : base(value) { }
    }

    // Omitted:
    // public class PythonBytes ...
    // public class PythonBytearray ...
    // public class PythonMemoryView ...

    public abstract class PythonSequence<TElement> : PyObject
    {
        public readonly TElement[] Elements;

        protected PythonSequence(TElement[] elements)
        {
            Elements = elements;
        }
        
        // public override T GetValue<T>() => (T) Convert.ChangeType(Value, typeof(T));
    }

    public class PythonList<TElement> : PythonSequence<TElement>
    {
        public PythonList(TElement[] elements) : base(elements) { }
    }

    public class PythonTuple<TElement> : PythonSequence<TElement>
    {
        public PythonTuple(TElement[] elements) : base(elements) { }
    }

    // Omitted:
    // public class PythonSet ...
    // public class PythonFrozenSet ...

    // TODO:
    // public class PythonDict ...
} // namespace Python