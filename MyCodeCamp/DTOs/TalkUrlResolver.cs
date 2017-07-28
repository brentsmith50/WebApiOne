using AutoMapper;
using MyCodeCamp.Data.Entities;
using Microsoft.AspNetCore.Http;
using MyCodeCamp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MyCodeCamp.DTOs
{
    public class TalkUrlResolver : IValueResolver<Talk, TalksDto, string>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public TalkUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Talk source, TalksDto destination, string destMember, ResolutionContext context)
        {
            var helper = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return helper.Link("GetTalk", new { moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id });
        }
    }
}
