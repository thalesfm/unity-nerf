using System.Linq;
using Sprache;

namespace UnityNeRF.Editor.IO
{
    internal readonly partial struct NpyFileHeader
    {
        public readonly string Descr;
        public readonly bool FortranOrder;
        public readonly int[] Shape;

        public NpyFileHeader(string descr, bool fortranOrder, int[] shape)
        {
            Descr = descr;
            FortranOrder = fortranOrder;
            Shape = shape;
        }

        public override string ToString()
        {
            string shape = string.Join(", ", Shape.Select(x => x.ToString()));
            return $"{{'descr': {Descr}, 'fortran_order': {FortranOrder}, 'shape': ({shape})}}";
        }
    }
} // namespace UnityNeRF.Editor.IO