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
using Piranha.Manager.Services;
using Piranha.Manager.Models;
using Piranha.Models;


namespace Wurd.Controllers
{

    [ApiController]
    [Route("api/wurd/[controller]")]
    [Authorize]
    public class CustomizeApiController : ControllerBase
    {
        private readonly IApi _api;
        private readonly PageService _service;


        public CustomizeApiController(IApi api, PageService service)
        {
            _api = api;
            _service = service;
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

        [Route("sites-list")]
        [HttpGet]
        [Authorize]
        public async Task<PageListModel> List()
        {
            var model = await _service.GetList();
            return model;
        }

        [HttpPost]
        [Route("GetPagesByIds")]
        public virtual async Task<IActionResult> GetByIds(List<Guid> ids)
        {
            var result = new List<PageBase>();
            foreach (var id in ids)
            {
                result.Add(await _api.Pages.GetByIdAsync<PageBase>(id));
            }

            return Ok(result);

        }
    }
}
