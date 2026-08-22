namespace backend.Models
{
    public class ReviewCreateRequest
    {
        public string? Name { get; set; } = "";
        public int Rating { get; set; }
        public string? Text { get; set; } = "";
    }
}
