using AeroChef.AI.Agents.Agents;
using Microsoft.AspNetCore.Mvc;

namespace AeroChef.AI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly ChefBot _chefBot;
        private readonly SafetyBot _safetyBot;
        private readonly OpsLead _opsLead;

        public AgentController(ChefBot chefBot, SafetyBot safetyBot, OpsLead opsLead)
        {
            _chefBot = chefBot;
            _safetyBot = safetyBot;
            _opsLead = opsLead;
        }

        [HttpPost("chef")]
        public async Task<IActionResult> ChefAgent([FromBody] string query)
        {
            var result = await _chefBot.ExecuteAsync(query);
            return Ok(new { Agent = "ChefBot", Response = result });
        }

        [HttpPost("safety")]
        public async Task<IActionResult> SafetyAgent([FromBody] string query)
        {
            var result = await _safetyBot.ExecuteAsync(query);
            return Ok(new { Agent = "SafetyBot", Response = result });
        }

        [HttpPost("ops")]
        public async Task<IActionResult> OpsAgent([FromBody] string query)
        {
            var result = await _opsLead.ExecuteAsync(query);
            return Ok(new { Agent = "OpsLead", Response = result });
        }
    }
}
