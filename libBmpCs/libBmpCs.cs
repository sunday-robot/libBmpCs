namespace libBmpCs
{
    public static class Bmp
    {
        public static (byte[], int, int) Load(string filePath)
        {
            var fs = File.OpenRead(filePath);
            LoadBitmapFileHeader(fs);
            var (biWidth, biHeight, biBitCount) = LoadBitmapInfoHeader(fs);
            Func<Stream, int, int, byte[]> loadData = biBitCount switch
            {
                24 => Load24,
                _ => throw new Exception($"unsupported bit count : {biBitCount}"),
            };
            var data = loadData(fs, (int)biWidth, (int)biHeight);

            fs.Close();
            return (data, (int)biWidth, (int)biHeight);
        }

        public static void Save(string filePath, byte[] data, int width, int height)
        {
            var fs = File.OpenWrite(filePath);
            SaveBitmapFileHeader(fs, width, height);
            SaveBitmapInfoHeader(fs, width, height);
            Save24(fs, data, width, height);
            fs.Close();
        }

        static void LoadBitmapFileHeader(Stream s)
        {
            s.Seek(2, SeekOrigin.Current);  // bfType "BM"
            s.Seek(4, SeekOrigin.Current);  // ファイルサイズ
            s.Seek(2, SeekOrigin.Current);  // bfReserved1
            s.Seek(2, SeekOrigin.Current);  // bfReserved2
            s.Seek(4, SeekOrigin.Current);  // ファイル先頭から画像データまでのオフセット
        }

        static void SaveBitmapFileHeader(Stream s, int width, int height)
        {
            var bfSize = 54 + (width * 3 + 3) / 4 * 4 * height;

            s.WriteByte((byte)'B'); s.WriteByte((byte)'M'); // bfType "BM"
            WriteUint(s, (uint)bfSize); // ファイルサイズ
            WriteUshort(s, 0);  // bfReserved1
            WriteUshort(s, 0);  // bfReserved2
            WriteUint(s, 54);   // ファイル先頭から画像データまでのオフセット
        }

        static (uint, uint, uint) LoadBitmapInfoHeader(Stream s)
        {
            s.Seek(4, SeekOrigin.Current);  // biSize 40
            var biWidth = ReadUint(s);
            var biHeight = ReadUint(s);
            s.Seek(2, SeekOrigin.Current);  // biPlanes 1
            var biBitCount = ReadUshort(s); // 色ビット数
            s.Seek(4, SeekOrigin.Current);  // biCompression
            s.Seek(4, SeekOrigin.Current);  // biSizeImage
            s.Seek(4, SeekOrigin.Current);  // biXPixPerMeter
            s.Seek(4, SeekOrigin.Current);  // biYPixPerMeter
            s.Seek(4, SeekOrigin.Current);  // biClrUsed
            s.Seek(4, SeekOrigin.Current);  // biCirImportant

            return (biWidth, biHeight, biBitCount);
        }

        static void SaveBitmapInfoHeader(Stream s, int width, int height)
        {
            var biSizeImage = (width * 3 + 3) / 4 * 4 * height;
            var bi_PixPerMeter = 3780;
            WriteUint(s, 40);   // ファイルサイズ
            WriteUint(s, (uint)width);
            WriteUint(s, (uint)height);
            WriteUshort(s, 1);  // biPlanes
            WriteUshort(s, 24); // biBitCount
            WriteUint(s, 0);    // biCompression
            WriteUint(s, (uint)biSizeImage);
            WriteUint(s, (uint)bi_PixPerMeter);
            WriteUint(s, (uint)bi_PixPerMeter);
            WriteUint(s, 0);    // biClrUsed
            WriteUint(s, 0);    // biClrImportant
        }

        static byte[] Load24(Stream s, int width, int height)
        {
            var data = new byte[width * height * 3];
            var gap = (4 - width * 3 % 4) % 4;
            for (var i = 0; i < height; i++)
            {
                s.Read(data, (height - 1 - i) * width * 3, width * 3);
                s.Seek(gap, SeekOrigin.Current);
            }

            return data;
        }

        static void Save24(Stream s, byte[] data, int width, int height)
        {
            var gap = (4 - width * 3 % 4) % 4;
            for (var i = 0; i < height; i++)
            {
                s.Write(data, (height - 1 - i) * width * 3, width * 3);
                for (var j = 0; j < gap; j++)
                    s.WriteByte(0);
            }
        }

        static ushort ReadUshort(Stream s)
        {
            var data = new byte[2];
            s.Read(data);
            return BitConverter.ToUInt16(data, 0);
        }

        static uint ReadUint(Stream s)
        {
            var data = new byte[4];
            s.Read(data);
            return BitConverter.ToUInt32(data, 0);
        }

        static void WriteUshort(Stream s, ushort data)
        {
            var bytes = BitConverter.GetBytes(data);
            s.Write(bytes);
        }

        static void WriteUint(Stream s, uint data)
        {
            var bytes = BitConverter.GetBytes(data);
            s.Write(bytes);
        }
    }
}