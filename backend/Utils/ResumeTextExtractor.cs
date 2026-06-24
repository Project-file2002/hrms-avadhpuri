using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace HRMS.API.Utils;

public static class ResumeTextExtractor
{
    public static string ExtractFromPdf(string path)
    {
        try
        {
            using var reader = new PdfDocument(new PdfReader(path));
            var text = new System.Text.StringBuilder();
            for (int i = 1; i <= reader.GetNumberOfPages(); i++)
            {
                var page = reader.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                text.Append(PdfTextExtractor.GetTextFromPage(page, strategy));
            }
            return text.ToString();
        }
        catch
        {
            return "";
        }
    }

    public static string ExtractFromDocx(string path)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return "";
            var text = new System.Text.StringBuilder();
            foreach (var para in body.Elements<Paragraph>())
                text.AppendLine(para.InnerText);
            return text.ToString();
        }
        catch
        {
            return "";
        }
    }

    public static string Extract(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => ExtractFromPdf(filePath),
            ".doc" or ".docx" => ExtractFromDocx(filePath),
            _ => ""
        };
    }
}
