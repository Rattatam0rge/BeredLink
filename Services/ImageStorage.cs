using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FeedbackMap.Services;

/// <summary>
/// Lagrer opplastede bilder på disk. Filtypen avgjøres av filens innhold (magic bytes), ikke av filnavnet
/// eller Content-Type fra klienten. Filene får tilfeldig navn.
/// </summary>
public class ImageStorage : IImageStorage
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private readonly string _root;

    public ImageStorage(IOptions<StorageOptions> options)
    {
        var configured = options.Value.UploadsPath;
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "uploads")
            : configured;
    }

    public async Task<ImageSaveResult> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file.Length == 0) return Fail("Bildet er tomt.");
        if (file.Length > MaxBytes) return Fail("Bildet er for stort. Maks 5 MB.");

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAtLeastAsync(header.AsMemory(), header.Length, throwOnEndOfStream: false, ct);

        var ext = Detect(header, read);
        if (ext is null) return Fail("Bruk et bilde i formatet JPEG, PNG eller WebP.");

        Directory.CreateDirectory(_root);
        var name = Guid.NewGuid().ToString("N") + ext;

        await using (var output = File.Create(Path.Combine(_root, name)))
        {
            await output.WriteAsync(header.AsMemory(0, read), ct);
            await input.CopyToAsync(output, ct);
        }

        return new ImageSaveResult(true, name, null);
    }

    public Stream? Open(string fileName)
    {
        var path = Path.Combine(_root, Path.GetFileName(fileName));
        return File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
    }

    public void Delete(string fileName)
    {
        try
        {
            var path = Path.Combine(_root, Path.GetFileName(fileName));
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException)
        {
            // Ikke kritisk: filen blir liggende
        }
    }

    public string ContentTypeFor(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

    private static ImageSaveResult Fail(string error) => new(false, null, error);

    private static string? Detect(byte[] h, int n)
    {
        if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (n >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 &&
            h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A) return ".png";
        if (n >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' &&
            h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P') return ".webp";
        return null;
    }
}
