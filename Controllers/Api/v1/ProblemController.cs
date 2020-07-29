﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Judge1.Models;
using Judge1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judge1.Controllers.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProblemController : ControllerBase
    {
        private readonly IProblemService _service;
        private readonly ILogger<ProblemController> _logger;

        public ProblemController(IProblemService service, ILogger<ProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ProblemInfoDto>>> ListProblems(int? pageIndex)
        {
            var privileged = User.IsInRole(ApplicationRoles.ProblemEditor);
            return Ok(await _service.GetPaginatedProblemInfosAsync(pageIndex, privileged));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProblemViewDto>> ViewProblem(int id)
        {
            try
            {
                var privileged = User.IsInRole(ApplicationRoles.ProblemEditor);
                return Ok(await _service.GetProblemViewAsync(id, privileged));
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e);
            }
            catch (Exception e)
            {
                return NotFound(e);
            }
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateProblem(ProblemEditDto dto)
        {
            try
            {
                var problem = await _service.CreateProblemAsync(dto);
                return Created(nameof(ViewProblem), problem);
            }
            catch (ValidationException e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProblem(ProblemEditDto dto)
        {
            try
            {
                var problem = await _service.UpdateProblemAsync(dto);
                return Ok(problem);
            }
            catch (ValidationException e)
            {
                return BadRequest(e);
            }
        }

        [HttpDelete]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteProblem(int id)
        {
            try
            {
                await _service.DeleteProblemAsync(id);
                return NoContent();
            }
            catch (ValidationException e)
            {
                return BadRequest(e);
            }
        }
    }
}