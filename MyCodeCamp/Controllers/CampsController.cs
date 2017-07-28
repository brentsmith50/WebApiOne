using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using AutoMapper;
using MyCodeCamp.DTOs;
using MyCodeCamp.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace MyCodeCamp.Controllers
{
    [Authorize]
    [EnableCors("AnyGET")]
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        #region Fields
        private ILogger<CampsController> logger;
        private ICampRepository repository;
        private IMapper mapper;
        #endregion

        #region Constructor
        public CampsController(ICampRepository repo, ILogger<CampsController> logger, IMapper mapper)
        {
            this.repository = repo;
            this.logger = logger;
            this.mapper = mapper;
        }
        #endregion

        #region Methods
        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = repository.GetAllCamps();
            return Ok(this.mapper.Map<IEnumerable<CampDto>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;

                if (includeSpeakers)
                {
                    camp = repository.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                    camp = repository.GetCampByMoniker(moniker);

                if (camp == null) return NotFound($"Camp {moniker} was not found");

                return Ok(this.mapper.Map<CampDto>(camp));
            }
            catch
            {
            }

            return BadRequest();
        }

        [EnableCors("Wildermuth")]
        [Authorize(Policy = "SuperUsers")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CampDto model)
        {
            try
            {
                logger.LogInformation("Creating a new Code Camp");

                var camp = this.mapper.Map<Camp>(model);

                repository.Add(camp);
                if (await repository.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
                    return Created(newUri, this.mapper.Map<CampDto>(camp));
                }
                else
                {
                    logger.LogWarning("Could not save Camp to the database");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Threw exception while saving Camp: {ex}");
            }

            return BadRequest();

        }

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampDto model)
        {
            try
            {
                var oldCamp = repository.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Could not find a camp with an ID of {moniker}");

                this.mapper.Map(model, oldCamp);

                if (await repository.SaveAllAsync())
                {
                    return Ok(this.mapper.Map<CampDto>(oldCamp));
                }
            }
            catch (Exception)
            {

            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var campToDelete = repository.GetCampByMoniker(moniker);
                if (campToDelete == null) return NotFound($"Could not find Camp with ID of {moniker}");

                repository.Delete(campToDelete);
                if (await repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
            }

            return BadRequest("Could not delete Camp");
        }
        #endregion
    }
}







