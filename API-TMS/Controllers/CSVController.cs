using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using API_TMS.Repository.Interface;
using API_TMS.Models;
using API_TMS.Dtos;

namespace API_TMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CSVController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskItemRepository _taskRepository;


        public CSVController(IUserRepository userRepository, ITaskItemRepository taskRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToCsv()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();

                var userDtos = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                }).ToList();

                var memoryStream = new MemoryStream();
                var writer = new StreamWriter(memoryStream);
                var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csv.WriteRecords(userDtos);
                writer.Flush(); 

                memoryStream.Position = 0; 

                return File(memoryStream, "text/csv", "users.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("taskcsv")]
        public async Task<IActionResult> ExportTaskAnalysisCsv()
        {
            try
            {
                var tasks = await _taskRepository.GetAllWithUsersAsync();

                var analysisData = tasks
                    .GroupBy(t => t.AssignedUser?.Email ?? "Unknown")
                    .Select(g => new TaskAnalysisDto
                    {
                        UserEmail = g.Key,
                        TotalTasks = g.Count(),
                        CompletedTasks = g.Count(t => t.Status == TStatus.Done),
                        InProgressTasks = g.Count(t => t.Status == TStatus.InProgress),
                        PendingTasks = g.Count(t => t.Status == TStatus.ToDo),
                    })
                    .ToList();

                var memoryStream = new MemoryStream();
                var writer = new StreamWriter(memoryStream);
                var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csv.WriteRecords(analysisData);
                writer.Flush();

                memoryStream.Position = 0;

                return File(memoryStream, "text/csv", "task_analysis.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
