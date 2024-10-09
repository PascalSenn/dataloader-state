using System.Collections.Generic;

namespace DataLoader.State.Example;

public class Blog
{
    public int BlogId { get; set; }

    public string Title { get; set; }

    public List<Post> Posts { get; } = new();
}