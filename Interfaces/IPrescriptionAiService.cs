using Microsoft.AspNetCore.Http;

namespace Midix.Services
{

    public interface IPrescriptionAiService
    {
        Task<string> AnalyzePrescriptionAsync(IFormFile imageFile);
    }
}
