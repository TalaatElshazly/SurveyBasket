namespace Bookify.Services;

public interface IPollService
{
    Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellation = default);
    Task<Poll?> GetAsync(int id, CancellationToken cancellation = default);
    Task<Poll> AddAsync(Poll poll ,CancellationToken cancellation = default);
    Task<bool> UpdateAsync(int id ,Poll poll,CancellationToken cancellation = default);
    Task<bool> DeleteAsync(int id ,CancellationToken cancellation = default);
    Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellation = default);
    Task<bool> IsTitleExistsAsync(string title, int? excludeId = null, CancellationToken cancellation=default);
}
