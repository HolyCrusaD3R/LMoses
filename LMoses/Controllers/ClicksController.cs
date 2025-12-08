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
            var counter = await _db.ClickCounters.FirstOrDefaultAsync();

            if (counter == null)
            {
                counter = new ClickCounter { Count = 1 };
                _db.ClickCounters.Add(counter);
            }
            else
            {
                counter.Count++;
            }

            await _db.SaveChangesAsync();
            return Ok(counter.Count);
        }

        [HttpGet]
        public async Task<ActionResult<int>> GetCount()
        {
            var counter = await _db.ClickCounters.FirstOrDefaultAsync();
            return counter?.Count ?? 0;
        }

        [HttpPost("reset")]
        public async Task<ActionResult> Reset()
        {
            var counter = await _db.ClickCounters.FirstOrDefaultAsync();
            if (counter != null)
            {
                counter.Count = 0;
                await _db.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
