using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = "";
        public int Rating { get; set; }
        public string Text { get; set; } = "";

        // Machine translation of Text into the other site language (null if
        // translation is unconfigured or the DeepL call failed — the review
        // still saves fine, it just won't have a translated version to show).
        public string? TranslatedText { get; set; }

        // Language Text was written in ("en" or "fr"), as detected by DeepL.
        public string? SourceLang { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
