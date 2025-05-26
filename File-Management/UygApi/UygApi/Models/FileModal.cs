namespace UygApi.Models
{
    public class FileModal
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public AppUser User { get; set; }
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Extension { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadOn { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
