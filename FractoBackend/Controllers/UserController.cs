using Fracto.Data;
using Fracto.Models;
using Fracto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public UserController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username and password are required.");

            if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                return BadRequest("Username already exists.");

            var user = new User
            {
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role ?? "User"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Registered successfully" });
        }
        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] Models.LoginRequest req)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
    if (user == null) return Unauthorized("Invalid username or password");

    bool passwordValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
    if (!passwordValid) return Unauthorized("Invalid username or password");

    // Generate short-lived JWT
    var token = _jwt.GenerateToken(user);

    // Generate refresh token in memory
    var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    // Store in HttpOnly cookie (session handling)
    Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(1) // refresh token valid for 1 day
    });
//refresh token lives in browser cookie until logout.
    return Ok(new { token, username = user.Username, role = user.Role, profileImage = user.ProfileImagePath });
}
[HttpPost("refresh-token")]
public IActionResult RefreshToken()
{
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized("No refresh token found");

    //  Simply issue new JWT — we trust the cookie
    // (If you want extra security, you can keep refresh token list in memory or Redis)
    var username = User?.Identity?.Name; // current user context
    if (username == null)
        return Unauthorized();

    var user = _db.Users.FirstOrDefault(u => u.Username == username);
    if (user == null) return Unauthorized();

    var newToken = _jwt.GenerateToken(user);
    return Ok(new { token = newToken });
}

[HttpPost("logout")]
public IActionResult Logout()
{
    Response.Cookies.Delete("refreshToken");
    return Ok(new { message = "Logged out successfully" });
}


        // [HttpPost("login")]
        // public async Task<IActionResult> Login([FromBody] Models.LoginRequest req)
        // {
        //     var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        //     if (user == null) return Unauthorized("Invalid username or password");

        //     bool passwordValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        //     if (!passwordValid) return Unauthorized("Invalid username or password");

        //     var token = _jwt.GenerateToken(user);
        //     return Ok(new { token, username = user.Username, role = user.Role, profileImage = user.ProfileImagePath });
        // }
        [HttpGet("all")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAllUsers()
{
    var users = await _db.Users
        .Select(u => new
        {
            u.UserId,
            u.Username,
            u.Role,
            u.ProfileImagePath
        })
        .ToListAsync();

    return Ok(users);
}
[HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] User updated)
{
    var user = await _db.Users.FindAsync(id);
    if (user == null) return NotFound();

    user.Username = updated.Username;
    user.Role = updated.Role;
    //  Optional: update password if provided
    if (!string.IsNullOrEmpty(updated.PasswordHash))
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updated.PasswordHash);
    }

    await _db.SaveChangesAsync();
    return Ok(new { message = "User updated successfully" });
}

[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _db.Users.FindAsync(id);
    if (user == null) return NotFound();

    _db.Users.Remove(user);
    await _db.SaveChangesAsync();

    return Ok(new { message = "User deleted successfully" });
}

[HttpGet("me")]
[Authorize] //  so only logged-in user can access
public async Task<IActionResult> GetMyProfile()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var user = await _db.Users.FindAsync(int.Parse(userId));
    if (user == null) return NotFound();

    return Ok(new {
        username = user.Username,
        role = user.Role,
        imagePath = user.ProfileImagePath
    });
}

        [HttpPost("upload-profile-image")]
        [Authorize] // must be logged in
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Create folder if not exists
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Create unique file name
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Get logged-in user
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Save file path to DB
            user.ProfileImagePath = $"/uploads/{fileName}";
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new { imagePath = user.ProfileImagePath });
        }

    }
}
