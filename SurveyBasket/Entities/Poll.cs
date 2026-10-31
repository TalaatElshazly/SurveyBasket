namespace Bookify.Entities;

public sealed class Poll
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; }= string.Empty;
    public bool IsPublished { get; set; }
    public DateTime StartsAt{ get; set; }
    public DateTime EndsAt { get; set; }

}
