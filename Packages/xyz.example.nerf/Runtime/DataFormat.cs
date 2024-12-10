using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using NumSharp;

// #pragma warning disable IDE1006 // Naming Styles

namespace UnityNeRF
{
    [Serializable]
    public struct DataFormat
    {
        public const int RGBA = 0;
        public const int SH   = 1;
        public const int SG   = 2;
        public const int ASG  = 3;

        public int format;
        public int basis_dim;

        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public int data_dim => 3 * basis_dim + 1;

        public DataFormat(int format, int basis_dim)
        {
            this.format = format;
            this.basis_dim = basis_dim;
        }

        public static DataFormat Parse(string str)
        {
            if (str is null)
                throw new ArgumentNullException();
            
            if (str == "RGBA")
                return new DataFormat(RGBA, -1);
            
            string suffix = string.Concat(str.SkipWhile(char.IsLetter));
            if (!int.TryParse(suffix, out int basis_dim))
                throw new FormatException($"Failed to parse data format \"{str}\"");
            
            if (str.StartsWith("SH"))
                return new DataFormat(SH, basis_dim);
            if (str.StartsWith("SG"))
                return new DataFormat(SG, basis_dim);
            if (str.StartsWith("ASG"))
                return new DataFormat(ASG, basis_dim);
            
            throw new FormatException($"Failed to parse data format \"{str}\"");
        }

        public override string ToString()
        {
            switch (format) {
            case RGBA:
                return "RGBA";
            case SH:
                return $"SH{basis_dim}";
            case SG:
                return $"SG{basis_dim}";
            case ASG:
                return $"ASG{basis_dim}";
            default:
                throw new Exception("Unreachable");
            }
        }
    }
} // namespace UnityNeRF
