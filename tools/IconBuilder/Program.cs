using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

if (args.Length != 2) throw new ArgumentException("Usage: IconBuilder input.png output.ico");
var sourcePath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);
var decoder = BitmapDecoder.Create(new Uri(sourcePath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
var source = decoder.Frames[0];
var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
var frames = new List<(int Size, byte[] Png)>();

foreach (var size in sizes)
{
    var scaled = new TransformedBitmap(source, new ScaleTransform((double)size / source.PixelWidth, (double)size / source.PixelHeight));
    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(scaled));
    using var stream = new MemoryStream();
    encoder.Save(stream);
    frames.Add((size, stream.ToArray()));
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
using var output = File.Create(outputPath);
using var writer = new BinaryWriter(output);
writer.Write((ushort)0); writer.Write((ushort)1); writer.Write((ushort)frames.Count);
var offset = 6 + frames.Count * 16;
foreach (var frame in frames)
{
    writer.Write((byte)(frame.Size == 256 ? 0 : frame.Size));
    writer.Write((byte)(frame.Size == 256 ? 0 : frame.Size));
    writer.Write((byte)0); writer.Write((byte)0);
    writer.Write((ushort)1); writer.Write((ushort)32);
    writer.Write(frame.Png.Length); writer.Write(offset);
    offset += frame.Png.Length;
}
foreach (var frame in frames) writer.Write(frame.Png);
Console.WriteLine($"Created {outputPath} with {frames.Count} PNG frames");
