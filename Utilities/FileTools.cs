using System.IO;

namespace Quest.Utilities;
public static class FileTools
{
    public static byte ReadHeader(BinaryReader reader)
    {
        Stream stream = reader.BaseStream;
        try
        {
            byte pad1 = reader.ReadByte();
            if (pad1 != 254)
            {
                if (stream.Position >= 1)
                    stream.Seek(-1, SeekOrigin.Current); // Reset if no header
                return 0;
            }

            byte version = reader.ReadByte();
            byte pad2 = reader.ReadByte();

            if (pad2 != 255)
            {
                if (stream.Position >= 3)
                    stream.Seek(-3, SeekOrigin.Current); // Reset if no header
                return 0;
            }
            return version;
        }
        catch (EndOfStreamException)
        {
            stream.Seek(0, SeekOrigin.Begin); // Reset if no header
            return 0;
        }
    }
}
