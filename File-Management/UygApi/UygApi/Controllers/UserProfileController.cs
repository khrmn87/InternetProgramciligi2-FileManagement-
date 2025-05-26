using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UygApi.Models;

namespace UygApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        private const string DefaultProfilePicture = "/Uploads/ProfilePictures/default.png";

        public UserProfileController(UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        // ✅ Profil fotoğrafı yükleme
        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya boş veya yüklenmedi.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads", "ProfilePictures");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Eski özel fotoğrafı sil (eğer varsa ve default değilse)
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl != DefaultProfilePicture)
                {
                    string oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                user.ProfilePictureUrl = $"/Uploads/ProfilePictures/{uniqueFileName}";
                await _userManager.UpdateAsync(user);

                return Ok(new { ProfilePictureUrl = user.ProfilePictureUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Profil fotoğrafı yükleme hatası: {ex.Message}");
            }
        }

        // ✅ Profil fotoğrafı alma
        [HttpGet("get-profile-picture")]
        public async Task<IActionResult> GetProfilePicture()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var profileUrl = string.IsNullOrEmpty(user.ProfilePictureUrl)
                ? DefaultProfilePicture
                : user.ProfilePictureUrl;

            return Ok(new { ProfilePictureUrl = profileUrl });
        }

        // ✅ Profil fotoğrafı silme
        [HttpDelete("delete-profile-picture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (string.IsNullOrEmpty(user.ProfilePictureUrl) || user.ProfilePictureUrl == DefaultProfilePicture)
            {
                return BadRequest("Varsayılan profil fotoğrafı silinemez.");
            }

            try
            {
                string fullPath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                user.ProfilePictureUrl = DefaultProfilePicture;
                await _userManager.UpdateAsync(user);

                return Ok("Profil fotoğrafı silindi ve varsayılan resim ayarlandı.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fotoğraf silme hatası: {ex.Message}");
            }
        }
    }
}
