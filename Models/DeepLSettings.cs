namespace backend.Models
{
    public class DeepLSettings
    {
        public string ApiKey { get; set; } = "";

        // Defaults to the DeepL free-tier endpoint; override for a Pro key
        // (api.deepl.com) via config.
        public string ApiUrl { get; set; } = "https://api-free.deepl.com/v2/translate";
    }
}
