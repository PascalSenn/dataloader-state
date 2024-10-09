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
    public static async Task<Connection<Post>> GetPostsAsync(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        PagingArguments arguments,
        CancellationToken cancellationToken)
    {
        return await batching.PostsByBlogId
            .WithPagingArguments(arguments)
            .LoadAsync(blog.BlogId, cancellationToken)
            .ToConnectionAsync();
    }

    /*

    The projection does not seem to work

    "message": "The LINQ expression 'DbSet<Post>()\n
            .Where(p => __keys_0\n.Contains(p.BlogId))\n
            .GroupBy(p => new Post{ \n        PostId = p.PostId, \n        Title = p.Title \n    }\n    .BlogId)'
            could not be translated.
            Either rewrite the query in a form that can be translated, or switch to client
            evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable',
            'ToList', or 'ToListAsync'.
            See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.",
    */
    [UsePaging]
    public static async Task<Connection<Post>> GetPostsProjectedAndPaged(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        PagingArguments arguments,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await batching.PostsByBlogIdProjected
            .WithPagingArguments(arguments)
            .Select(selection)
            .LoadAsync(blog.BlogId, cancellationToken)
            .ToConnectionAsync();
    }

    /**
    This is does not work because the context data is not porpagated inside of the paging
    dataloader

    "message": "The state `where` is not available on the DataLoader.",

    */
    [UsePaging]
    public static async Task<Connection<Post>> GetPostsFilteredAndPaged(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        PagingArguments arguments,
        CancellationToken cancellationToken)
    {
        return await batching.PostsByBlogIdWithFilter
            .Where(x => x.Title.Contains("GraphQL"))
            .WithPagingArguments(arguments)
            .LoadAsync(blog.BlogId, cancellationToken)
            .ToConnectionAsync();
    }

    /// Also does not work
    [UsePaging]
    public static async Task<Connection<Post>> GetPostsFilteredAndProjectedAndPaged(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        PagingArguments arguments,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await batching.PostsByBlogIdWithFilterProjected
            .Where(x => x.Title.Contains("GraphQL"))
            .WithPagingArguments(arguments)
            .Select(selection)
            .LoadAsync(blog.BlogId, cancellationToken)
            .ToConnectionAsync();
    }

    /// This works
    public static async Task<Post[]?> GetPostsFiltered(
        [Parent("BlogId")] Blog blog,
        IBlogBatchingContext batching,
        CancellationToken cancellationToken)
    {
        return await batching.PostsFiltered
            .Where(x => x.Title.Contains("GraphQL"))
            .LoadAsync(blog.BlogId, cancellationToken);
    }
}