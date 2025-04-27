using System.IO;
using System.IO.Compression;
using Godot;

namespace ProjectNeva.Main.Utils;

[GlobalClass]
public partial class ImageHelper : GodotObject
{
    public const int CanvasWidth = 826;
    public const int CanvasHeight = 648;
    
    public static int GetCanvasWidth() { return CanvasWidth; }
    public static int GetCanvasHeight() { return CanvasHeight; }

    public static Image CreateEmptyImage()
    {
        return Image.CreateEmpty(
            CanvasWidth,
            CanvasHeight,
            false,
            Image.Format.Rgba8
        );
    }

    public static Image CreateImageFromCompressed(byte[] compressedImageData)
    {
        return Image.CreateFromData(
            CanvasWidth,
            CanvasHeight,
            false,
            Image.Format.Rgba8,
            Decompress(compressedImageData)
        );
    }
    
    public static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var brotliStream = new BrotliStream(compressedStream, CompressionLevel.Optimal);
        brotliStream.Write(data, 0, data.Length);
        brotliStream.Close();
        return compressedStream.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        brotliStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}