using System.IO;

namespace UnityNeRF.Editor.IO
{
    public static class NpyFile
    {
        public static NpyReader OpenRead(string path)
        {
            Stream stream = File.OpenRead(path);
            return new NpyReader(stream);
        }
    }
} // namespace UnityNeRF.Editor.IO