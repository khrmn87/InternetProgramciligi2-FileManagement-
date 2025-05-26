using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UygApi.Models;

namespace UygApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileModalController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FileModalController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileModal>>> GetUserFiles()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return await _context.Files
                .Include(f => f.Category)
                .Where(f => f.UserId == userId || _context.FileShares.Any(sf => sf.FileId == f.Id && sf.SharedWithUserId == userId))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileModal>> GetFile(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var file = await _context.Files.Include(f => f.Category).FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
                return NotFound();

            if (file.UserId != userId && !_context.FileShares.Any(sf => sf.FileId == id && sf.SharedWithUserId == userId))
                return Forbid();

            return file;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<FileModal>> UploadFile([FromForm] IFormFile file, [FromForm] int categoryId, [FromForm] string description = "")
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya boş.");

            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var fileModal = new FileModal
            {
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                Extension = Path.GetExtension(file.FileName),
                FileType = file.ContentType,
                Description = description,
                FilePath = filePath,
                UploadedBy = User.Identity.Name ?? "Anonymous",
                UploadOn = DateTime.Now,
                CategoryId = categoryId,
                UserId = userId
            };

            _context.Files.Add(fileModal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFile), new { id = fileModal.Id }, fileModal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromForm] string description, [FromForm] int? categoryId)
        {
            var fileModal = await _context.Files.FindAsync(id);
            if (fileModal == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (fileModal.UserId != userId)
                return Forbid();

            if (!string.IsNullOrEmpty(description))
                fileModal.Description = description;

            if (categoryId.HasValue)
                fileModal.CategoryId = categoryId.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var fileModal = await _context.Files.FindAsync(id);
            if (fileModal == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (fileModal.UserId != userId)
                return Forbid();

            if (System.IO.File.Exists(fileModal.FilePath))
                System.IO.File.Delete(fileModal.FilePath);

            _context.Files.Remove(fileModal);
            await _context.SaveChangesAsync();

            return Ok("Dosya silindi.");
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var fileModal = await _context.Files.FindAsync(id);

            if (fileModal == null)
                return NotFound();

            if (fileModal.UserId != userId && !_context.FileShares.Any(sf => sf.FileId == id && sf.SharedWithUserId == userId))
                return Forbid();

            if (!System.IO.File.Exists(fileModal.FilePath))
                return NotFound("Fiziksel dosya bulunamadı.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(fileModal.FilePath, FileMode.Open))
                await stream.CopyToAsync(memory);

            memory.Position = 0;
            return File(memory, fileModal.FileType, fileModal.FileName + fileModal.Extension);
        }

        [HttpPost("share")]
        public async Task<IActionResult> ShareFile([FromBody] ShareFileRequest request)
        {
            var file = await _context.Files.FindAsync(request.FileId);
            if (file == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (file.UserId != userId) return Forbid();

            foreach (var targetUserId in request.UserIds)
            {
                if (!_context.FileShares.Any(sf => sf.FileId == request.FileId && sf.SharedWithUserId == targetUserId))
                {
                    _context.FileShares.Add(new FileShareWith
                    {
                        FileId = request.FileId,
                        UserId = userId, // Dosya sahibinin ID'si
                        SharedWithUserId = targetUserId // Dosyanın paylaşıldığı kullanıcının ID'si
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Dosya paylaşıldı.");
        }

        //[HttpPost("unshare")]
        //public async Task<IActionResult> UnshareFile([FromBody] UnshareFileRequest request)
        //{
        //    var file = await _context.Files.FindAsync(request.FileId);
        //    if (file == null) return NotFound();

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (file.UserId != userId) return Forbid();

        //    var shared = await _context.FileShares.FirstOrDefaultAsync(sf => sf.FileId == request.FileId && sf.SharedWithUserId == request.UserId);
        //    if (shared != null)
        //    {
        //        _context.FileShares.Remove(shared);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok("Paylaşım kaldırıldı.");
        //}

        [HttpPost("unshare")]
        public async Task<IActionResult> UnshareFileMultiple([FromBody] UnshareMultipleRequest request)
        {
            var file = await _context.Files.FindAsync(request.FileId);
            if (file == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (file.UserId != userId) return Forbid();

            // Sorgulanan kullanıcı kimliklerine sahip tüm paylaşımları bulun
            var sharedEntries = await _context.FileShares
                .Where(sf => sf.FileId == request.FileId && request.UserIds.Contains(sf.SharedWithUserId))
                .ToListAsync();

            if (sharedEntries.Any())
            {
                _context.FileShares.RemoveRange(sharedEntries);
                await _context.SaveChangesAsync();
                return Ok($"{sharedEntries.Count} paylaşım kaldırıldı.");
            }

            return Ok("Kaldırılacak paylaşım bulunamadı.");
        }

        [HttpPost("unshare-all")]
        public async Task<IActionResult> UnshareFileAll([FromBody] UnshareAllRequest request)
        {
            var file = await _context.Files.FindAsync(request.FileId);
            if (file == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (file.UserId != userId) return Forbid();

            // Bu dosyaya ait tüm paylaşımları bulun
            var allSharedEntries = await _context.FileShares
                .Where(sf => sf.FileId == request.FileId)
                .ToListAsync();

            if (allSharedEntries.Any())
            {
                _context.FileShares.RemoveRange(allSharedEntries);
                await _context.SaveChangesAsync();
                return Ok($"Dosya için toplam {allSharedEntries.Count} paylaşım kaldırıldı.");
            }

            return Ok("Kaldırılacak paylaşım bulunamadı.");
        }

        private bool FileModalExists(int id) => _context.Files.Any(e => e.Id == id);

        public class ShareFileRequest
        {
            public int FileId { get; set; }
            public List<string> UserIds { get; set; } = new();
        }

        public class UnshareFileRequest
        {
            public int FileId { get; set; }
            public string UserId { get; set; }
        }

        public class UnshareMultipleRequest
        {
            public int FileId { get; set; }
            public List<string> UserIds { get; set; } = new();
        }

        public class UnshareAllRequest
        {
            public int FileId { get; set; }
        }
    }
}