using AutoMapper;
using MyCodeCamp.Data.Entities;
using Microsoft.AspNetCore.Http;
using MyCodeCamp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MyCodeCamp.DTOs
{
    public class SpeakerUrlResolver : IValueResolver<Speaker, SpeakersDto, string>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public SpeakerUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Speaker source, SpeakersDto destination, string destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("SpeakerGet", new { moniker = source.Camp.Moniker, id = source.Id });
        }
    }
}
