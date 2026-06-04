using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfToJson.Interface;
using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PdfToJson.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertJsonToPdfController : ControllerBase
    {
        private readonly IConvertJsonToPdf _context;
        public ConvertJsonToPdfController(IConvertJsonToPdf context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a PDF");

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var json = await _context.ExtractPdfAsync(filePath);

            await _context.SaveJsonToDatabase(file.FileName, json);

            return Ok(new
            {
                message = "Processed successfully",
                data = json
            });
        }

    }
}