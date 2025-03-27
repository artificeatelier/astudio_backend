namespace backend.Models
{
    public class ContactUsPostModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }

    }
    public static class ContactUsData{
        public static List<ContactUsPostModel> Data = new();
    }
}
