using System.IO;

namespace DwinIcoTool
{
    /// <summary>
    /// Simple JPG header reader.
    /// </summary>
    public class JPGReader
    {
        /// <summary>
        /// Gets the width and height from the header of a stream.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public static void GetJfifDimensions(Stream stream, out int width, out int height)
        {
            if (!stream.CanRead)
            {
                width = 0;
                height = 0;
                return;
            }

            byte[] buffer = new byte[8];
            int read;
            while ((read = stream.ReadByte()) > -1 && stream.CanRead)
            {
                // SOF0 is 0xFF 0xC0
                if (read == 0xFF)
                {
                    if (stream.ReadByte() == 0xC0)
                    {
                        stream.Read(buffer, 0, 8);
                        break;
                    }
                }
            }

            height = buffer[3] << 8 | buffer[4];
            width = buffer[5] << 8 | buffer[6];
        }
    }
}
