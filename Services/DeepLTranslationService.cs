using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.Extensions.Options;

namespace backend.Services
{
    // Site has exactly two languages (en/fr). To translate a review's text
    // into "the other one" without knowing the source ahead of time:
    //   1. Ask DeepL to translate into EN. Its response includes the
    //      detected source language for free.
    //   2. If the detected source was already EN, the review needs a
    //      second call translating into FR instead.
    //   3. Otherwise the EN result from step 1 *is* the translation.
    public class DeepLTranslationService : ITranslationService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public DeepLTranslationService(HttpClient http, IOptions<DeepLSettings> options)
        {
            _http = http;
            _apiKey = options.Value.ApiKey;
            _apiUrl = options.Value.ApiUrl;
        }

        public async Task<TranslationResult?> TranslateAsync(string text)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return null;

            var toEnglish = await CallDeepLAsync(text, "EN");
            if (toEnglish == null)
                return null;

            if (string.Equals(toEnglish.Value.DetectedSourceLang, "EN", StringComparison.OrdinalIgnoreCase))
            {
                var toFrench = await CallDeepLAsync(text, "FR");
                if (toFrench == null)
                    return null;

                return new TranslationResult("en", toFrench.Value.Text);
            }

            return new TranslationResult(toEnglish.Value.DetectedSourceLang.ToLowerInvariant(), toEnglish.Value.Text);
        }

        private async Task<(string Text, string DetectedSourceLang)?> CallDeepLAsync(string text, string targetLang)
        {
            var form = new Dictionary<string, string>
            {
                ["auth_key"] = _apiKey,
                ["text"] = text,
                ["target_lang"] = targetLang
            };

            using var response = await _http.PostAsync(_apiUrl, new FormUrlEncodedContent(form));
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var translation = doc.RootElement.GetProperty("translations")[0];
            var resultText = translation.GetProperty("text").GetString();
            var detected = translation.GetProperty("detected_source_language").GetString();

            if (resultText == null || detected == null)
                return null;

            return (resultText, detected);
        }
    }
}
