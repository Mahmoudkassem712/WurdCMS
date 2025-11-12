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
using Piranha.Manager;


namespace Wurd.Controllers
{

    [ApiController]
    [Route("wurd/[controller]")]
    public class CustomizeApiController : ControllerBase
    {
        private readonly IApi _api;
        private readonly PageService _service;


        public CustomizeApiController(IApi api, PageService service)
        {
            _api = api;
            _service = service;
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var page = await _api.Pages.GetBySlugAsync(slug);
            if (page == null)
                return NotFound();

            return Ok(page);
        }


        [Route("sites-list")]
        [HttpGet]
        public async Task<IActionResult> SiteList()
        {
            var model = await _service.GetList();

            return Ok(model);
        }

        [HttpGet]
        [Route("get-page")]
        public virtual async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _api.Pages.GetByIdAsync<PageBase>(id));
        }

        [HttpPost]
        [Route("get-pages")]
        public virtual async Task<IActionResult> GetByIds(List<Guid> ids)
        {
            var result = new List<PageBase>();
            foreach (var id in ids)
            {
                result.Add(await _api.Pages.GetByIdAsync<PageBase>(id));
            }

            return Ok(result);

        }

        [Route("child-pages-list")]
        [HttpGet]
        public async Task<IActionResult> ChildPageList(Guid siteId, Guid pageId)
        {
            var sites = await _api.Sites.GetAllAsync();
            var pages = new List<PageListModel.PageItem>();
            var sitePages = await _api.Pages.GetAllAsync(siteId);

           var result = sitePages.Where(x => x.ParentId == pageId).ToList();

            return Ok(result);
        }


        [HttpGet]
        [Route("ping")]
        public virtual async Task<IActionResult> ping()
        {
            return Ok("Welcome to wurd cms");
        }


        ///////////////////////////////////////////////

        //[Route("sites-list")]
        //[HttpGet]
        //public async Task<IActionResult> SiteList()
        //{
        //    var model = await _service.GetList();

        //    return Ok(model);

        //    //var model = new PageListModel
        //    //{
        //    //    Sites = (await _api.Sites.GetAllAsync())
        //    //          .OrderByDescending(s => s.IsDefault)
        //    //          .Select(s => new PageListModel.PageSite
        //    //          {
        //    //              Id = s.Id,
        //    //              Title = s.Title,
        //    //              Slug = "/",
        //    //              EditUrl = "manager/site/edit/"
        //    //          }).ToList(),
        //    //};

        //    //foreach (var site in model.Sites)
        //    //{
        //    //    site.Pages.AddRange(await GetPageStructure(site.Id));
        //    //}

        //    //return Ok(model);

        //}


        //private async Task<List<PageListModel.PageItem>> GetPageStructure(Guid siteId)
        //{
        //    var pages = new List<PageListModel.PageItem>();

        //    // Get the configured expanded levels
        //    var expandedLevels = 0;
        //    using (var config = new Config(_api))
        //    {
        //        expandedLevels = config.ManagerExpandedSitemapLevels;
        //    }

        //    // Get the sitemap and transform
        //    var sitemap = await _api.Sites.GetSitemapAsync(siteId, false);
        //    var drafts = await _api.Pages.GetAllDraftsAsync(siteId);
        //    foreach (var item in sitemap)
        //    {
        //        pages.Add(MapRecursive(siteId, item, 0, expandedLevels, drafts));
        //    }
        //    return pages;
        //}

        //private PageListModel.PageItem MapRecursive(Guid siteId, SitemapItem item, int level, int expandedLevels, IEnumerable<Guid> drafts)
        //{

        //    var model = new PageListModel.PageItem
        //    {
        //        Id = item.Id,
        //        SiteId = siteId,
        //        Title = item.MenuTitle,
        //        TypeName = item.PageTypeName,
        //        Published = item.Published.HasValue ? item.Published.Value.ToString("yyyy-MM-dd") : null,
        //        Status = drafts.Contains(item.Id) ? PageListModel.PageItem.Draft :
        //            !item.Published.HasValue ? PageListModel.PageItem.Unpublished : "",
        //        EditUrl = "manager/page/edit/",
        //        IsExpanded = level < expandedLevels,
        //        IsCopy = item.OriginalPageId.HasValue,
        //        IsRestricted = item.Permissions.Count > 0,
        //        IsScheduled = item.Published.HasValue && item.Published.Value > DateTime.Now,
        //        IsUnpublished = !item.Published.HasValue,
        //        Permalink = item.Permalink
        //    };

        //    foreach (var child in item.Items)
        //    {
        //        model.Items.Add(MapRecursive(siteId, child, level + 1, expandedLevels, drafts));
        //    }
        //    return model;
        //}

    }
}
