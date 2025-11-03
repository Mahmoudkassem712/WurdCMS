using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Wurd.Models;
using Piranha;
using Piranha.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Wurd.Controllers
{

    [ApiController]
    [Route("api/wurd/[controller]")]
    [Authorize]
    public class DuaController : ControllerBase
    {
        private readonly IApi _api;

        public DuaController(IApi api)
        {
            _api = api;
        }

        // GET: api/dua/slug/adeya
        [HttpGet("slug/{slug}")]
        [Authorize]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var page = await _api.Pages.GetBySlugAsync(slug);
            if (page == null)
                return NotFound();

            return Ok(page);
        }


        // مثال: endpoint يحتاج دور معين (مثلاً SysAdmin)
        [HttpGet("admin-only")]
        [Authorize(Roles = "SysAdmin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "مرحبًا يا Admin! هذا محتوى سري" });
        }
    }
}
