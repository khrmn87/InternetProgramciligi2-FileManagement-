using Microsoft.AspNetCore.Identity;

namespace UygApi.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; }
        public string ProfilePictureUrl { get; set; } = "/Uploads/ProfilePictures/default.png";
        public ICollection<FileModal>? Files { get; set; }
    }
}
