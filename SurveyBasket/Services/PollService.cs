namespace Bookify.Services;

public class PollService(ApplicationDbContext _dbContext) : IPollService
{
   
    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellation)
    {
        return await _dbContext.Polls.AsNoTracking().ToListAsync(cancellation);
    }

    public async Task<Poll?> GetAsync(int id,CancellationToken cancellation)
    {
        return await _dbContext.Polls.FindAsync(new object[] { id },cancellation);
    }
    public async Task<Poll> AddAsync(Poll poll,CancellationToken cancellation)
    {
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync(cancellation);
        return poll;
    }

    public async Task<bool> UpdateAsync(int id, Poll poll,CancellationToken cancellation)
    {
        var currentPoll = await  GetAsync(id,cancellation);
        if(currentPoll is null)
        {
            return false;
        }
        currentPoll.Title = poll.Title;
        currentPoll.Summary = poll.Summary;
        currentPoll.StartsAt = poll.StartsAt;
        currentPoll.EndsAt = poll.EndsAt;
        await _dbContext.SaveChangesAsync(cancellation);
        return true;
    }
    public async Task< bool> DeleteAsync(int id,CancellationToken cancellation)
    {
       
        var rowsDeleted = await _dbContext.Polls
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellation);
        return rowsDeleted>0;
    }
    public async Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellation)
    {
        var Poll = await GetAsync(id, cancellation);
        if (Poll is null)
        {
            return false;
        }
        Poll.IsPublished=!Poll.IsPublished;
       
        await _dbContext.SaveChangesAsync(cancellation);
        return true;
    }

    public async Task<bool> IsTitleExistsAsync(string title, int? excludeId = null, CancellationToken cancellation=default)
    {
        return await _dbContext.Polls.AnyAsync(x => x.Title == title&& (!excludeId.HasValue || x.Id != excludeId.Value), cancellation);
    }
}
