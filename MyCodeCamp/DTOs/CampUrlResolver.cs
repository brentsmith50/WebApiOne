using AutoMapper;
using MyCodeCamp.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;

namespace MyCodeCamp.DTOs
{
    public class CampUrlResolver : IValueResolver<Camp, CampDto, string>
    {
        private IHttpContextAccessor httpContextAccessor;

        public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Camp source, CampDto destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)this.httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("CampGet", new { moniker = source.Moniker });
        }
    }
}
