using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Wurd.Models;
using Piranha.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.SqlServer.Server;
using Google.Apis.Auth;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            var jwt = GenerateJwtToken(user);
            return Ok(new { token = jwt });
        }

        return Unauthorized();
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("role", "User")
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("google-auth")]
    public async Task<IActionResult> LoginByGoogle([FromBody] LoginModel model)
    {

        try
        {

            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(model.IdToken);
            

            // 1.  verify idToken from Google
            //var settings = new GoogleJsonWebSignature.ValidationSettings
            //{
            //    Audience = new[] { "13629534461-9o99ro3077e5s8djeq1r707o6bvl419j.apps.googleusercontent.com" }  // Use the Android/iOS Client ID from Google Console
            //};
            //var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);
            
            var email = jwtToken.Payload["email"].ToString();
            var name = jwtToken.Payload["name"].ToString();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                // أضف صلاحيات API
                await _userManager.AddToRoleAsync(user, "User");

                // CRITICAL: Reload user with roles
                user = await _userManager.FindByEmailAsync(email);
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google token");
        }
    }
}

