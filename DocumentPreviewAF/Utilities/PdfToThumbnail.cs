using DocumentPreviewAF.Models;

namespace DocumentPreviewAF.Utilities;
public class PdfToThumbnail
{
    public Stream Convert(Stream inputStream, ImageSize imageSize)
    {
        var pdf = new PdfDocument(inputStream);

        var firstPage = pdf.CopyPage(0);

        var temp = Path.GetTempFileName();

        var size = ImageSizeExtensions.SizeMap[imageSize];

        var imagePaths = firstPage.RasterizeToImageFiles(temp, size.Width, size.Height);

        return File.OpenRead(imagePaths[0]);
    }
}

