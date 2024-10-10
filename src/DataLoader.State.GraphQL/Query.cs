using System.Threading;
using System.Threading.Tasks;
using GreenDonut.Projections;
using HotChocolate;
using HotChocolate.Execution.Processing;
using HotChocolate.Pagination;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using HotChocolate.Types.Relay;

namespace DataLoader.State.Example;

public static class Operations
{
    [NodeResolver]
    [Query]
    public static async Task<Blog?> GetBlogByIdAsync(
        int blogId,
        IBlogBatchingContext batching,
        CancellationToken cancellationToken)
    {
        return await batching.BlogById.LoadAsync(blogId, cancellationToken);
    }

    [Query]
    public static async Task<Blog?> GetBlogByIdProjectedAsync(
        int blogId,
        IBlogBatchingContext batching,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await batching.BlogByIdProjected
            .Select(selection)
            .LoadAsync(blogId, cancellationToken);
    }
}

[ObjectType<Blog>]
public static partial class BlogType
{
    [UsePaging]
    public static async Task<Connection<Post>> GetGraphQLPosts(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        PagingArguments arguments,
        CancellationToken cancellationToken)
    {
        return await batching.PostsByBlogId
            .Where(x => x.Title.Contains("GraphQL"))
            .WithPagingArguments(arguments)
            .LoadAsync(blog.BlogId, cancellationToken)
            .ToConnectionAsync();
    }
}