using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PdfToJson.Interface;
using System.Text.Json;
using UglyToad.PdfPig;

namespace PdfToJson.Repository
{
    public class ConvertJsonToPdfRepo : IConvertJsonToPdf
    {
        private readonly IConfiguration _configuration;
        private readonly DocumentIntelligenceClient _client;

        public ConvertJsonToPdfRepo(IConfiguration configuration)
        {
            _configuration = configuration;
            var endpoint = _configuration["AzureDocumentIntelligence:Endpoint"];
            var key = _configuration["AzureDocumentIntelligence:Key"];

            _client = new DocumentIntelligenceClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));
        }

        public async Task<string> ExtractPdfAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);

            var operation = await _client.AnalyzeDocumentAsync(
                waitUntil.Completed,
                "prebuilt-layout",
                stream);

            var result = operation.Value;

            var output = result.Pages.Select(p => new
            {
                Page = p.PageNumber,
                Lines = p.Lines.Select(l => l.Content)
            });

            return JsonSerializer.Serialize(output, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public async Task SaveJsonToDatabase(string fileName, string jsonData)
        {
            string connectionString =
                _configuration.GetConnectionString("Conn");

            using SqlConnection connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync();

            string query = @"
                INSERT INTO PdfJsonData (FileName, JsonData)
                VALUES (@FileName, @JsonData)";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FileName", fileName);
            command.Parameters.AddWithValue("@JsonData", jsonData);

            await command.ExecuteNonQueryAsync();
        }

    }
}
