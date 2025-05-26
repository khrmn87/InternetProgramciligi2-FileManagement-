namespace UygApi.DTOs
{
    public class StarredDto
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public DateTime StarredOn { get; set; } = DateTime.Now;
    }
}
