namespace UygApi.Models
{
    public class FileShareWith
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string UserId { get; set; }
        public FileModal File { get; set; }
        public string SharedWithUserId { get; set; }
        public AppUser User { get; set; }
    }
}
