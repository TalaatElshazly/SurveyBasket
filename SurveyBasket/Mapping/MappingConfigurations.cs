using Bookify.Contracts.Polls;

namespace Bookify.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Poll,PollResponse>()
            .Map(dest => dest.Notes, src => src.Summary);
    }
}
