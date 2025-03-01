namespace GrpcService.Utilities;

using SkiaSharp;

public static class ImageGenerator
{
    public static byte[] GenerateRandomJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        var paint = new SKPaint
        {
            Color = new SKColor((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256))
        };
        canvas.DrawRect(0, 0, width, height, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
    }
}
