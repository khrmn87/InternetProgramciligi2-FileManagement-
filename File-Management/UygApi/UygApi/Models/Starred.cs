namespace UygApi.Models
{
    public class Starred
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public FileModal? File { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; } 
        public AppUser? User { get; set; }
        public DateTime StarredOn { get; set; } = DateTime.Now;
    }
}