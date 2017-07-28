using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Attributes;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    [ApiVersion("1.0")]
    [ApiVersion("1.0")]
    public class SpeakersController : BaseController
    {
        #region Fields
        protected ILogger<SpeakersController> logger;
        protected ICampRepository repository;
        protected IMapper mapper;
        protected UserManager<CampUser> userManager;
        #endregion

        #region Constructor
        public SpeakersController(ICampRepository repository, ILogger<SpeakersController> logger, IMapper mapper, UserManager<CampUser> userManager)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
            this.userManager = userManager;
        }
        #endregion

        #region Methods
        [HttpGet]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? this.repository.GetSpeakersByMonikerWithTalks(moniker) : this.repository.GetSpeakersByMoniker(moniker);
            return Ok(this.mapper.Map<IEnumerable<SpeakersDto>>(speakers));
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? this.repository.GetSpeakersByMonikerWithTalks(moniker) : this.repository.GetSpeakersByMoniker(moniker);
            return Ok(new { count = speakers.Count(), results = this.mapper.Map<IEnumerable<SpeakersDto>>(speakers) });
        }


        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks ? this.repository.GetSpeakerWithTalks(id) : this.repository.GetSpeaker(id);
            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker)
            {
                return BadRequest("The selected speaker is not in the camp.");
            }
            return Ok(this.mapper.Map<SpeakersDto>(speaker));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(string moniker, [FromBody]SpeakersDto speakerDto)
        {
            try
            {
                var camp = this.repository.GetCampByMoniker(moniker);
                if (camp == null)
                {
                    return BadRequest("Could not find the camp.");
                }

                var speaker = this.mapper.Map<Speaker>(speakerDto);
                speaker.Camp = camp;


                var campUser = await this.userManager.FindByNameAsync(this.User.Identity.Name);

                if (campUser != null)
                {
                    speaker.User = campUser;
                    this.repository.Add(speaker);

                    if (await repository.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                        return Created(url, this.mapper.Map<SpeakersDto>(speaker));
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.LogError($"Threw exception while saving speaker: {ex}");
            }

            return BadRequest("Could not add new Speaker.");

        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody]SpeakersDto speakerDto)
        {
            try
            {
                var speaker = this.repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker)
                {
                    return BadRequest("The selected speaker and Camp do not match.");
                }

                if (speaker.User.UserName !=  this.User.Identity.Name)
                {
                    return Forbid();
                }

                this.mapper.Map(speakerDto, speaker);

                if (await repository.SaveAllAsync())
                {
                    return Ok(this.mapper.Map<SpeakersDto>(speaker));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Threw exception while updating speaker: {ex}");
            }

            return BadRequest("Couldn't update Speaker.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = repository.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name)
                {
                    return Forbid();
                }

                this.repository.Delete(speaker);
                if (await repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Threw exception while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete Speaker");
        }
        #endregion
    }
}
