/*
# DWIN_ICO
# -  Dissect and create DWIN .ico files for their LCD displays.
#
#  Copyright (c) 2020 Brent Burton
#
#  This program is free software: you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation, either version 3 of the License, or
#  (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#----------------------------------------------------------------
#
# This is not a normal Microsoft .ICO file, but it has a similar
# structure for containing a number of icon images. Each icon is
# a small JPG file.
#
# The file has a directory header containing fixed-length
# records, and each record points to its data at an offset later
# in the file.
#
# The directory entries are 16 bytes each, and the entire
# directory is 4KB (0 - 0x1000). This supports 256 entries.
#
# Multibyte values are in Big Endian format.
#
# Header: (offset 0x0)
#              W    H     offset  ?? len  ?? ??
#   Entry 0:  xxxx xxxx 00001000 xx 10a2 00 00000000
#   Entry 1:  xxxx xxxx 000020a2 xx 0eac 00 00000000
#   Entry 2:  xxxx xxxx 00002f4e xx 0eaa 00 00000000
#   ...
# 0x00001000: ffd8 ffe1 0018 ... jpeg exif and data follow .. ffd9
# 0x000020a2: ffd8 ffe1 ...
# ...rest of ICO entries' data...
#
# Header structure:
# Offset Len  What
#      0   2  width
#      2   2  height
#      4   4  file byte position from SEEK_BEG
#      8   3  length of data
#     11   5  ??? all zeroes
#
# Other notes:
# * The index of each icon corresponds to the Icon number in dwin.h
# * One exception is number 39: that header entry is blank, and dwin.h
#   does not define a name for 39. This is specially handled to
#   prevent reordering stock icons.
 *
 * 
 * 
 * Please see https://github.com/b-pub/dwin-ico-tools/
 *
 * The following program is the equivalent in C# .NET
 * 
 * Usage: Drag and drop the file to the executable or run from the command line
 * 
 * Example 1:   DwinIcoTool.exe 9.ICO       // This will extract the "9.ICO" file into "out" folder
 * Example 2:   DwinIcoTool.exe out         // This will create a ICO file named "out"
*/

using System;
using System.IO;

namespace DwinIcoTool
{
    public static class Program
    {
        public static readonly short MAX_ENTRIES = 256;
        public static readonly byte DATA_SIZE = 16;

        private static readonly IcoEntry[] entries = new IcoEntry[MAX_ENTRIES];

        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    if (Path.GetExtension(args[0]).ToUpperInvariant() == ".ICO")
                        OpenICO(args[0]);
                    else
                        Console.WriteLine("File extension must be .ICO");
                }
                else if (Directory.Exists(args[0]))
                {
                    CreateICO(args[0]);
                }
                else
                {
                    Console.WriteLine("The directory or file specified does not exist.");
                    Console.WriteLine($"Args[{args.Length}] = {args[0]}");
                }
            }
            else
            {
                Console.WriteLine("No arguments passed. Drag and drop an ICO file on the app executable.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Opens an ICO file.
        /// </summary>
        /// <param name="fileName">File.</param>
        public static void OpenICO(string fileName)
        {
            using (FileStream fstream = File.OpenRead(fileName))
                ParseHeader(fstream);
        }

        /// <summary>
        /// Parses the header and extracts the images.
        /// </summary>
        /// <param name="stream">ICO file stream.</param>
        public static void ParseHeader(Stream stream)
        {
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                byte[] buffer = new byte[DATA_SIZE];
                stream.Read(buffer, 0, buffer.Length);
                entries[i] = new IcoEntry(buffer);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                IcoEntry entry = entries[i];
                if (entry?.Length == 0)
                    continue;

                Console.WriteLine($"Index: {i} - {entry}");
                Extract(stream, entry, $"out/{i}_{(IconNames)i}.jpg");
            }

            Console.WriteLine("All files are inside the \"out\" directory. You can later drop that folder to create a new ICO file.");
        }

        /// <summary>
        /// Extract an image.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        /// <param name="entry">Entry of the image.</param>
        /// <param name="outFileName">Filename to use for the image.</param>
        public static void Extract(Stream stream, IcoEntry entry, string outFileName)
        {
            if (entry == null || entry.Length == 0 || entry.Length >= stream.Length)
                return;

            byte[] buffer = new byte[entry.Length];
            stream.Position = entry.Offset;
            stream.Read(buffer, 0, buffer.Length);

            string dir = Path.GetDirectoryName(outFileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using(FileStream file = File.Create(outFileName))
            {
                file.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Creates an .ICO file.
        /// </summary>
        /// <param name="dir">Directory name.</param>
        public static void CreateICO(string dir)
        {
            FileInfo[] infos = new DirectoryInfo(dir).GetFiles();
            foreach (FileInfo file in infos)
            {
                using (Stream reader = file.OpenRead())
                {
                    int.TryParse(file.Name.Split('_')[0], out int index);

                    IcoEntry entry = new IcoEntry(reader);
                    entries[index] = entry;
                }
            }

            // Create .ICO
            string dirName = Path.GetFileName(dir);
            using (FileStream stream = File.Create(dirName + ".ICO"))
            {
                Console.WriteLine($"Creating {dirName}.ICO...");

                // Write header
                int offset = MAX_ENTRIES * DATA_SIZE;
                for(int i = 0; i < MAX_ENTRIES; i++)
                {
                    byte[] buffer = new byte[DATA_SIZE];
                    IcoEntry entry = entries[i];
                    if (entry != null)
                    {
                        entry.SetOffset(offset);
                        offset += entry.Data.Length;

                        buffer = entry.ToByteArray();
                    }

                    stream.Write(buffer, 0, buffer.Length);
                }

                // Write images
                foreach (IcoEntry entry in entries)
                {
                    if (entry != null)
                        stream.Write(entry.Data, 0, entry.Data.Length);
                }

                Console.WriteLine($"Output file is \"{dirName}.ICO\"!");
            }
        }
    }
}
