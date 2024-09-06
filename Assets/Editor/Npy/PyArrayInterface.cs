using System;

internal readonly struct PyArrayInterface
{
    public readonly Type TypeKind;
    public readonly int ItemSize;
    public readonly int Flags;
    public readonly int[] Shape;
    public readonly int[] Strides;
    public readonly Array Data;
    public readonly object Descr;
}