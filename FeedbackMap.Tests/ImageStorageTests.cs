using FeedbackMap.Services;
using Microsoft.AspNetCore.Http;

namespace FeedbackMap.Tests;

public class ImageStorageTests
{
    private static IFormFile File(byte[] content, string fileName = "bilde.jpg")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "Image", fileName);
    }

    private static byte[] WithHeader(byte[] header, int totalLength = 200)
    {
        var bytes = new byte[totalLength];
        header.CopyTo(bytes, 0);
        return bytes;
    }

    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0 };
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] WebP = { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P' };

    [Theory]
    [MemberData(nameof(ValidImages))]
    public async Task Save_AcceptsJpegPngAndWebP_AndChoosesExtensionFromContent(byte[] header, string expectedExt)
    {
        using var f = new Fixture();

        // filnavnet lyver (.txt), men innholdet avgjør
        var result = await f.Images.SaveAsync(File(WithHeader(header), "navn.txt"));

        Assert.True(result.Success);
        Assert.EndsWith(expectedExt, result.FileName);
        using var stream = f.Images.Open(result.FileName!);
        Assert.NotNull(stream);
    }

    public static IEnumerable<object[]> ValidImages() => new[]
    {
        new object[] { Jpeg, ".jpg" },
        new object[] { Png, ".png" },
        new object[] { WebP, ".webp" }
    };

    [Fact]
    public async Task Save_RejectsNonImage_EvenWithJpgExtension()
    {
        using var f = new Fixture();
        var text = System.Text.Encoding.UTF8.GetBytes("<script>alert(1)</script> dette er ikke et bilde");

        var result = await f.Images.SaveAsync(File(text, "ondsinnet.jpg"));

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.False(Directory.Exists(f.TempDir) && Directory.GetFiles(f.TempDir).Length > 0);
    }

    [Fact]
    public async Task Save_RejectsEmptyFile()
    {
        using var f = new Fixture();

        Assert.False((await f.Images.SaveAsync(File(Array.Empty<byte>()))).Success);
    }

    [Fact]
    public async Task Save_RejectsTooLargeFile()
    {
        using var f = new Fixture();
        var big = WithHeader(Jpeg, (int)ImageStorage.MaxBytes + 1);

        var result = await f.Images.SaveAsync(File(big));

        Assert.False(result.Success);
        Assert.Contains("stort", result.Error);
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        using var f = new Fixture();
        var saved = await f.Images.SaveAsync(File(WithHeader(Jpeg)));

        f.Images.Delete(saved.FileName!);

        Assert.Null(f.Images.Open(saved.FileName!));
    }

    [Fact]
    public void Open_PathTraversal_DoesNotEscapeUploadFolder()
    {
        using var f = new Fixture();

        Assert.Null(f.Images.Open("../../etc/passwd"));
    }
}
