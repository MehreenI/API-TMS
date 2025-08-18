using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public AnnouncementsController(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetAll()
        {
            try
            {
                var announcements = await _announcementRepository.GetAllAsync();
                return Ok(announcements);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Announcement>> GetById(int id)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(id);
                if (announcement == null)
                    return NotFound("Announcement not found");

                return Ok(announcement);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Announcement>> Create([FromBody] Announcement model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdAnnouncement = await _announcementRepository.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = createdAnnouncement.Id }, createdAnnouncement);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Announcement model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.Id = id;
                await _announcementRepository.UpdateAsync(model);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _announcementRepository.DeleteAsync(id);
                if (!result)
                    return NotFound("Announcement not found");

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }
    }
}


