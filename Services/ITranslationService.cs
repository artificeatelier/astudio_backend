using System.Threading.Tasks;

namespace backend.Services
{
    public record TranslationResult(string SourceLang, string TranslatedText);

    public interface ITranslationService
    {
        // Returns null when translation isn't configured or the call fails —
        // callers treat that as "no translation available", not an error.
        Task<TranslationResult?> TranslateAsync(string text);
    }
}
