using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MyCodeCamp.Controllers;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.DTOs
{
    public class TalkLinksResolver : IValueResolver<Talk, TalksDto, ICollection<LinkDto>>
    {
        private IHttpContextAccessor httpContextAccessor;

        public TalkLinksResolver(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ICollection<LinkDto> Resolve(Talk source, TalksDto destination, ICollection<LinkDto> destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)this.httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];

            return new List<LinkDto>
            {
                new LinkDto
                {
                    Rel = "Self",
                    Href = url.Link("GetTalk", new { moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id})
                },
                new LinkDto
                {
                    Rel = "Update",
                    Href = url.Link("UpdateTalk", new { moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id}),
                    Verb = "PUT"
                },
                new LinkDto
                {
                    Rel = "Speaker",
                    Href = url.Link("SpeakerGet", new { moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id})
                }
            };

        }
    }
}
