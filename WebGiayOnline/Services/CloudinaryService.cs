using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string imageUrl, string publicId)> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        // Lấy đuôi file để kiểm tra định dạng
        var extension = Path.GetExtension(file.FileName).ToLower();

        var isAnimated = extension == ".gif" || extension == ".webp" || extension == ".webm";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "avatars",
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true,
            // Không dùng Transformation nếu là ảnh động
            Transformation = isAnimated ? null : new Transformation().Width(200).Height(200).Crop("fill")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        throw new Exception("Tải ảnh lên Cloudinary thất bại.");
    }

    public async Task DeleteImageAsync(string publicId)
    {
        if (!string.IsNullOrEmpty(publicId))
        {
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
