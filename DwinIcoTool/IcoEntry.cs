using System;
using System.IO;

namespace DwinIcoTool
{
    /// <summary>
    /// Represents an entry in the header of an ICO file that can contain the image data.
    /// </summary>
    public class IcoEntry
    {
        /// <summary>
        /// Width of the image.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Position within the file.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Lenght of the data within the file.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// ?? data in the header entry.
        /// </summary>
        public byte[] ExtraBytes { get; private set; } = new byte[5];

        /// <summary>
        /// Size of the image.
        /// </summary>
        public int Size { get { return Width * Height; } }

        /// <summary>
        /// Image data of the entry.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initialize from a byte array.
        /// </summary>
        /// <param name="data">Byte array to parse.</param>
        public IcoEntry(byte[] data)
        {
            Parse(data);
        }

        /// <summary>
        /// Initialize from a stream.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        public IcoEntry(Stream stream)
        {
            JPGReader.GetJfifDimensions(stream, out int width, out int height);
            Width = width;
            Height = height;
            Length = stream.Length;

            stream.Position = 0;
            byte[] data = new byte[Length];
            stream.Read(data, 0, (int)Length);
            Data = data;
        }

        /// <summary>
        /// Parses the header data into an IcoEntry object.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>Returns true if the parse was successfull; otherwise, false.</returns>
        public bool Parse(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"{nameof(data)} cannot be null.");
            }

            if(data.Length != Program.DATA_SIZE)
            {
                throw new ArgumentException($"{nameof(data)} size must be 16 bytes.");
            }

            Width = data[0] << 8 | data[1];
            Height = data[2] << 8 | data[3];
            Offset = data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];

            Length = data[8] << 16 | data[9] << 8 | data[10];

            ExtraBytes[0] = data[11];
            ExtraBytes[1] = data[12];
            ExtraBytes[2] = data[13];
            ExtraBytes[3] = data[14];
            ExtraBytes[4] = data[15];

            return true;
        }

        /// <summary>
        /// Turns this IcoEntry into a byte array.
        /// </summary>
        /// <returns>Byte array.</returns>
        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[16];
            buffer[0] = (byte)(Width >> 8);
            buffer[1] = (byte)Width;

            buffer[2] = (byte)(Height >> 8);
            buffer[3] = (byte)Height;

            buffer[4] = (byte)(Offset >> 24);
            buffer[5] = (byte)(Offset >> 16);
            buffer[6] = (byte)(Offset >> 8);
            buffer[7] = (byte)Offset;

            buffer[8] = (byte)(Length >> 16);
            buffer[9] = (byte)(Length >> 8);
            buffer[10] = (byte)Length;

            buffer[11] = ExtraBytes[0];
            buffer[12] = ExtraBytes[1];
            buffer[13] = ExtraBytes[2];
            buffer[14] = ExtraBytes[3];
            buffer[15] = ExtraBytes[4];

            return buffer;
        }

        /// <summary>
        /// Sets the offset where the image Data is within the file.
        /// </summary>
        /// <param name="offset">Image data offset.</param>
        public void SetOffset(int offset)
        {
            Offset = offset;
        }

        public override string ToString()
        {
            return $"{Width}x{Height} - {Length} bytes - offset: {Offset}";
        }
    }
}
