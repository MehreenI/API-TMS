using API_TMS.Models;
using API_TMS.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplateRepository emailTemplateRepository;

        public EmailTemplatesController(IEmailTemplateRepository _emailTemplateRepository)
        {
            emailTemplateRepository = _emailTemplateRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailTemplate>>> GetAll()
        {
            try
            {
                var emailTemplates = await emailTemplateRepository.GetAllAsync();
                return Ok(emailTemplates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred"+ ex.Message);
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmailTemplate model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.Id = id;
                await emailTemplateRepository.UpdateAsync(model);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error occurred");
            }
        }

      
    }
}


