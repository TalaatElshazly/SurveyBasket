using Bookify.Contracts.Polls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookify.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api-bucket")]
public class PollsController(IPollService pollService/*,IMapper mapper*/) : ControllerBase
{
    //private readonly IMapper _mapper = mapper;
    [HttpGet("")]
    public async Task <IActionResult> GetAll(CancellationToken cancellation)
    {
        var polls =await pollService.GetAllAsync(cancellation);
        var response = polls.Adapt <IEnumerable<PollResponse>>();//_mapper.Map<IEnumerable<PollResponse>>(poll);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task< IActionResult> Get([FromRoute]int id , CancellationToken cancellation)
    {
        var poll = await pollService.GetAsync(id, cancellation);
        if(poll is null)
        {
            return NotFound();
        }

        var response = poll.Adapt<PollResponse>();//_mapper.Map<PollResponse>(poll);
        return Ok(response);
    }

    [HttpPost("")]
    public async Task< IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellation)
    {
        var isTitleExists = await pollService.IsTitleExistsAsync(request.Title,null, cancellation);

        if (isTitleExists)
        {
            return Conflict(new { message = "There is already a survey with the same title. Please choose a different title." });
        }

        var addedPoll = await pollService.AddAsync(request.Adapt<Poll>(), cancellation/*_mapper.Map<Poll>(request)*/);
        var response = addedPoll.Adapt<PollResponse>();//_mapper.Map<PollResponse>(addedPoll);
        return CreatedAtAction(nameof(Get),
            new { id = addedPoll.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task< IActionResult> Update([FromRoute]int id,[FromBody]PollRequest request, CancellationToken cancellation)
    {
        var isTitleExists = await pollService.IsTitleExistsAsync(request.Title, id, cancellation);
        if (isTitleExists)
        {
            return Conflict(new { message = "There is already a survey with the same title. Please choose a different title." });
        }
        var isUpdated = await pollService.UpdateAsync(id,request.Adapt<Poll>(), cancellation /*_mapper.Map<Poll>(request)*/);
        if (!isUpdated)
        {
            return NotFound();
        }
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task< IActionResult> Delete([FromRoute]int id, CancellationToken cancellation)
    {
        var isDeleted = await pollService.DeleteAsync(id, cancellation);
        if (!isDeleted)
        {
            return NotFound();
        }
        return NoContent();
    }
    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePuplish([FromRoute] int id, CancellationToken cancellation)
    {
        var isUpdated = await pollService.TogglePublishStatusAsync(id, cancellation /*_mapper.Map<Poll>(request)*/);
        if (!isUpdated)
        {
            return NotFound();
        }
        return NoContent();
    }





    //Test1
    [Authorize(Roles = "User,Admin")]
    [HttpGet("TestUserAndAdmin")]
    public IActionResult GetTest()
    {
        return Ok(new[] { "Poll 1", "Poll 2" });
    }
    //Test2
    [Authorize(Roles = "Admin")]
    [HttpPost("TestAdmin")]
    public IActionResult CreateTest()
    {
        return Ok("Poll created successfully by Admin.");
    }
    //Test3
    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        return Ok(new { Message = "You’re logged in successfully, and your token is working perfectly!" });
    }
}