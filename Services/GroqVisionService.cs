using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Midix.Services
{
    /// <summary>
    /// Calls the Groq Vision API (llama-4-scout-17b-16e-instruct) to analyse a
    /// prescription image and return a structured JSON result matching
    /// <c>StructuredPrescriptionAnalysis</c>.
    /// </summary>
    public class GroqVisionService : IPrescriptionAiService
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const string GroqChatEndpoint = "https://api.groq.com/openai/v1/chat/completions";
        private const string VisionModel       = "meta-llama/llama-4-scout-17b-16e-instruct";

        /// <summary>
        /// System prompt that instructs Groq to return ONLY a JSON object whose
        /// shape matches <c>StructuredPrescriptionAnalysis</c>.
        /// </summary>
        private const string SystemPrompt = """
            You are a clinical pharmacist assistant that reads prescription images.
            Your task is to analyse the provided prescription image and extract all relevant information.

            You MUST respond with ONLY a single, valid JSON object — no markdown, no code fences, no explanation text.
            The JSON object must conform exactly to this schema:

            {
              "doctorSpecialty":       "<string: medical specialty of the prescribing doctor, e.g. Cardiology>",
              "suspectedDiagnosis":    "<string: probable diagnosis inferred from the medications>",
              "generalPrecautions":    "<string: overall safety warnings or advice for the patient>",
              "rawMedicalSummaryEn":   "<string: plain-English narrative summary of the prescription>",
              "medications": [
                {
                  "name":            "<string: generic or brand name>",
                  "dosageForm":      "<string: e.g. tablet, capsule, syrup, injection>",
                  "dosage":          "<string: strength per dose, e.g. 500 mg>",
                  "frequency":       "<string: how often to take it, e.g. twice daily>",
                  "duration":        "<string: total treatment period, e.g. 7 days>",
                  "instructionsAr":  "<string: patient-facing instructions written in Arabic>"
                }
              ]
            }

            Rules:
            - If a field cannot be determined from the image, use an empty string "".
            - The "medications" array must contain one object per medication line on the prescription.
            - "instructionsAr" must always be written in Arabic.
            - Do NOT wrap the JSON in markdown code blocks.
            - Do NOT include any text before or after the JSON object.
            """;

        // ── Dependencies ─────────────────────────────────────────────────────────

        private readonly HttpClient       _httpClient;
        private readonly IConfiguration   _configuration;
        private readonly ILogger<GroqVisionService> _logger;

        // ── Constructor (IHttpClientFactory pattern) ──────────────────────────────

        public GroqVisionService(
            IHttpClientFactory httpClientFactory,
            IConfiguration     configuration,
            ILogger<GroqVisionService> logger)
        {
            _httpClient    = httpClientFactory.CreateClient(nameof(GroqVisionService));
            _configuration = configuration;
            _logger        = logger;
        }

        // ── Interface implementation ──────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<string> AnalyzePrescriptionAsync(IFormFile imageFile)
        {
            // 1. Read image bytes and encode to base64 ─────────────────────────
            string base64Image;
            string mimeType = imageFile.ContentType; // e.g. "image/jpeg"

            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                base64Image = Convert.ToBase64String(memoryStream.ToArray());
            }

            string dataUri = $"data:{mimeType};base64,{base64Image}";

            // 2. Build the Groq chat-completion request body ───────────────────
            var requestBody = new
            {
                model = VisionModel,
                messages = new object[]
                {
                    // System turn — strict JSON-only instruction
                    new
                    {
                        role    = "system",
                        content = SystemPrompt
                    },
                    // User turn — the prescription image
                    new
                    {
                        role    = "user",
                        content = new object[]
                        {
                            new
                            {
                                type      = "text",
                                text      = "Please analyse this prescription image and return the structured JSON."
                            },
                            new
                            {
                                type      = "image_url",
                                image_url = new { url = dataUri }
                            }
                        }
                    }
                },
                temperature    = 0.1,   // Low temperature for deterministic, structured output
                max_tokens     = 2048,
                response_format = new { type = "json_object" } // Enforce JSON mode
            };

            string jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // 3. Prepare the HTTP request ──────────────────────────────────────
            string apiKey = _configuration["OpenAiSettings:ApiKey"]
                ?? throw new InvalidOperationException(
                    "Groq API key is missing. Add 'OpenAiSettings:ApiKey' to appsettings.json or environment variables.");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, GroqChatEndpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // 4. Send the request ──────────────────────────────────────────────
            _logger.LogInformation("Sending prescription image to Groq Vision API (model: {Model}).", VisionModel);

            HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Groq API returned {StatusCode}: {ErrorBody}",
                    response.StatusCode, errorBody);

                throw new InvalidOperationException(
                    $"Groq API error {(int)response.StatusCode}: {errorBody}");
            }

            // 5. Extract the model's message content ───────────────────────────
            string responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);

            string analysisJson = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? throw new InvalidOperationException(
                    "Groq API returned an empty content field.");

            _logger.LogInformation("Groq Vision analysis completed successfully.");

            return analysisJson; // Raw JSON string → stored in Prescription.AiAnalysisResult
        }
    }
}
