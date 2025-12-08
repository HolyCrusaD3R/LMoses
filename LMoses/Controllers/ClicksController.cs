using LMoses.Core.Models;
using LMoses.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMoses.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClicksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ClicksController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Increment()
        {
            var click = new Click
            {
                Timestamp = DateTime.UtcNow
            };
            _db.Clicks.Add(click);
            await _db.SaveChangesAsync();

            // Return total clicks count
            var count = await _db.Clicks.CountAsync();
            return Ok(count);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Click>>> GetClicks()
        {
            var clicks = await _db.Clicks
                .OrderByDescending(c => c.Timestamp)
                .ToListAsync();
            return Ok(clicks);
        }

        [HttpPost("reset")]
        public async Task<ActionResult> Reset()
        {
            _db.Clicks.RemoveRange(_db.Clicks);
            await _db.SaveChangesAsync();
            return Ok();
        }

    }
}
