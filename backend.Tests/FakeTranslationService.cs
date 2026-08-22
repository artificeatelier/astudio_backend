using System.Threading.Tasks;
using backend.Services;

// Test double for ITranslationService: returns whatever result was configured
// (default null, i.e. "translation not configured/available"), never makes a
// real network call.
public class FakeTranslationService : ITranslationService
{
    private readonly TranslationResult? _result;

    public FakeTranslationService(TranslationResult? result = null)
    {
        _result = result;
    }

    public Task<TranslationResult?> TranslateAsync(string text) => Task.FromResult(_result);
}
