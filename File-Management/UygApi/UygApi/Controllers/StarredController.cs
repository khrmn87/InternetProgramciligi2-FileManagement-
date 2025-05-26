using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UygApi.DTOs;
using UygApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UygApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StarredController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public StarredController(AppDbContext context, IMapper mapper, UserManager<AppUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost("star")]
        public async Task<ActionResult<StarredDto>> StarFile(StarredDto starredDto)
        {
            try
            {
                // Validation
                if (starredDto == null)
                    return BadRequest("Star data is required");

                if (string.IsNullOrWhiteSpace(starredDto.UserId))
                    return BadRequest("UserId is required");

                if (starredDto.FileId <= 0)
                    return BadRequest("Valid FileId is required");

                // User kontrolü
                var user = await _userManager.FindByIdAsync(starredDto.UserId);
                if (user == null)
                    return NotFound("User not found");

                // File kontrolü ve dosya bilgilerini al
                var file = await _context.Files.FindAsync(starredDto.FileId);
                if (file == null)
                    return NotFound("File not found");

                // Zaten yıldızlanmış mı kontrolü
                var alreadyStarred = await _context.Starreds
                    .AnyAsync(s => s.UserId == starredDto.UserId && s.FileId == starredDto.FileId);

                if (alreadyStarred)
                {
                    return BadRequest("This file has already been starred by the user.");
                }

                // FileName ve Username'i güvenli şekilde belirle
                var fileName = string.IsNullOrWhiteSpace(starredDto.FileName)
                    ? (file.FileName + file.Extension)
                    : starredDto.FileName.Trim();

                var username = string.IsNullOrWhiteSpace(starredDto.Username)
                    ? (user.UserName ?? "Unknown")
                    : starredDto.Username.Trim();

                // Starred entity'yi oluştur - TÜM GEREKLİ ALANLARI SET ET
                var starred = new Starred
                {
                    FileId = starredDto.FileId,
                    UserId = starredDto.UserId,
                    FileName = fileName,        // GEREKLİ ALAN - SET EDİLDİ
                    Username = username,        // GEREKLİ ALAN - SET EDİLDİ
                    StarredOn = DateTime.UtcNow
                };

                _context.Starreds.Add(starred);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<StarredDto>(starred);
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in StarFile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new { message = "An error occurred while starring the file", error = ex.Message });
            }
        }

        [HttpGet("get-starred-files/{userId}")]
        public async Task<ActionResult<IEnumerable<StarredDto>>> GetStarredFiles(string userId)
        {
            var starreds = await _context.Starreds
                .Where(s => s.UserId == userId)
                .Include(s => s.File)
                .Include(s => s.User)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<StarredDto>>(starreds));
        }

        [HttpDelete("unstar/{fileId}/{userId}")]
        public async Task<IActionResult> UnstarFile(int fileId, string userId)
        {
            var starred = await _context.Starreds
                .FirstOrDefaultAsync(s => s.FileId == fileId && s.UserId == userId);

            if (starred == null)
                return NotFound("Starred file not found");

            _context.Starreds.Remove(starred);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File unstarred successfully" });
        }
    }
}