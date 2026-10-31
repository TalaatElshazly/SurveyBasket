/*using Bookify.Contracts.Request;
using System.Collections;

namespace Bookify.Mapping;

public static class ContractMapping
{
    public static PollResponse MapToResponse(this Poll poll)
    {
        return new()
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description
        };
    }
    public static IEnumerable <PollResponse> MapToResponse(this IEnumerable< Poll> poll)
    {
        return poll.Select(MapToResponse);
    }
    public static Poll MapToPoll (this CreatePollRequest request)
    {
        return new()
        {
            Title =request.Title,
            Description = request.Description
        };
    }
}
*/