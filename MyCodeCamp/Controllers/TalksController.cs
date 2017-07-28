using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Attributes;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers/{speakerId}/talks")]
    [ValidateModel]
    public class TalksController : BaseController
    {
        #region Fields
        private ILogger<CampsController> logger;
        private ICampRepository repository;
        private IMapper mapper;
        private IMemoryCache memoryCache;
        #endregion

        #region Constructor
        public TalksController(ICampRepository repository, ILogger<CampsController> logger, IMapper mapper, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }
        #endregion

        #region Methods
        [HttpGet]
        public IActionResult Get(string moniker, int speakerId)
        {
            var talks = this.repository.GetTalks(speakerId);
            if (talks.Any(t => t.Speaker.Camp.Moniker != moniker))
            {
                return BadRequest("The talks are invalid for the selected speaker.");
            }
            return Ok(this.mapper.Map<IEnumerable<TalksDto>>(talks));
        }

        [HttpGet("{id}", Name = "GetTalk")]
        public IActionResult Get(string moniker, int speakerId, int id)
        {
            if (Request.Headers.ContainsKey("If-None-Match"))
            {
                var oldETag = Request.Headers["If-None-Match"].First();
                if (this.memoryCache.Get($"Talk-{id}-{oldETag}" ) != null)
                {
                    return StatusCode((int)HttpStatusCode.NotModified);
                }
            }

            var talk = this.repository.GetTalk(id);
            if (talk.Speaker.Id != speakerId || talk.Speaker.Camp.Moniker != moniker)
            {
                return BadRequest("Invalid talk for the selected speaker.");
            }

            this.AddETag(talk);

            return Ok(this.mapper.Map<TalksDto>(talk));
        }

        private void AddETag(Talk talk)
        {
            var etag = Convert.ToBase64String(talk.RowVersion);
            Response.Headers.Add("ETag", etag);
            this.memoryCache.Set($"Talk-{talk.Id}-{etag}", talk);
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, int speakerId, [FromBody] TalksDto talksDto)
        {
            try
            {
                var speaker = this.repository.GetSpeaker(speakerId);
                if (speaker != null)
                {
                    var talk = this.mapper.Map<Talk>(talksDto);
                    talk.Speaker = speaker;
                    this.repository.Add(talk);

                    if (await this.repository.SaveAllAsync())
                    {
                        AddETag(talk);
                        return Created(Url.Link("GetTalk",
                            new { moniker = moniker, speakerId = speakerId, id = talk.Id }),
                            this.mapper.Map<TalksDto>(talk));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An exception was thrown while saving the Talk: {ex}");
            }
            return BadRequest("Did not save Talk, an error occured.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int speakerId, int id, [FromBody] TalksDto talksDto)
        {
            try
            {
                var talk = this.repository.GetTalk(id);
                if (talk == null)
                {
                    return NotFound();
                }

                if (Request.Headers.ContainsKey("If-Match"))
                {
                    var eTag = Request.Headers["If-Match"].First();
                    if (eTag != Convert.ToBase64String(talk.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                this.mapper.Map(talksDto, talk);
                if (await repository.SaveAllAsync())
                {
                    AddETag(talk);
                    return Ok(this.mapper.Map<TalksDto>(talk));
                }
            }
            catch (Exception)
            {
                this.logger.LogError("An Exception was thrown while updating the Talk.");
            }

            return BadRequest("Couldn't update Talk");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int speakerId, int id)
        {
            try
            {
                var talk = this.repository.GetTalk(id);
                if (talk == null)
                {
                    return NotFound();
                }

                if (Request.Headers.ContainsKey("If-Match"))
                {
                    var eTag = Request.Headers["If-Match"].First();
                    if (eTag != Convert.ToBase64String(talk.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                this.repository.Delete(talk);  

                if (await repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"An Exception was thrown during delete: {ex}");
            }

            return BadRequest("Could not delete Talk");
        }
        #endregion
    }
}
