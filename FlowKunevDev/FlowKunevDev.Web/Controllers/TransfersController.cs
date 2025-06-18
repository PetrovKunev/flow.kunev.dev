using Microsoft.AspNetCore.Mvc;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly IAccountTransferService _transferService;

        public TransfersController(IAccountTransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var transfers = await _transferService.GetAllAsync(userId);
            return Ok(transfers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var transfer = await _transferService.GetByIdAsync(id, userId);
            if (transfer == null) return NotFound();
            return Ok(transfer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateAccountTransferDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.Identity?.Name ?? string.Empty;
            var result = await _transferService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var success = await _transferService.DeleteAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
