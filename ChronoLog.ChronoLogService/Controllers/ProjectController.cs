using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models.DisplayObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChronoLog.ChronoLogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;
    private readonly IProjectService _projectService;
    
    public ProjectController(ILogger<ProjectController> logger, IProjectService projectService)
    {
        _logger = logger;
        _projectService = projectService;
    }
    
    [HttpGet]
    public async Task<List<ProjectModel>> Get()
    {
        return await _projectService.ListProjectsAsync();
    }

    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }
    
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }
    
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }
    
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}