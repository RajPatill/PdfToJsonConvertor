namespace PdfToJson.Interface
{
    public interface IConvertJsonToPdf
    {
        Task<string> ExtractPdfAsync(string filePath);
        Task SaveJsonToDatabase(string fileName, string jsonData);


    }
}
