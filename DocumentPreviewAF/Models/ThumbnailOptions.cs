namespace DocumentPreviewAF.Models;

public class ThumbnailOptions
{
    public ImageSize ImageSize { get; set; }
    public string? Name { get; set; }

    public override string? ToString()
    {
        return $"{Path.GetFileNameWithoutExtension(Name)}-{ImageSize.GetWidth()}x{ImageSize.GetHeight()}";
    }
}
