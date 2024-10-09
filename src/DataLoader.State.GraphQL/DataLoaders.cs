using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using GreenDonut.Projections;
using HotChocolate.Data;
using HotChocolate.Pagination;
using Microsoft.EntityFrameworkCore;

namespace DataLoader.State.Example;

[DataLoaderGroup("BlogBatchingContext")]
public static class DataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<int, Blog>> GetBlogByIdAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        CancellationToken cancellationToken) =>
        await context.Blogs
            .Where(x => keys.Contains(x.BlogId))
            .ToDictionaryAsync(x => x.BlogId, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Blog>> GetBlogByIdProjectedAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        ISelectorBuilder selector,
        CancellationToken cancellationToken) =>
        await context.Blogs
            .Where(x => keys.Contains(x.BlogId))
            .Select(selector.TryCompile<Blog>()!)
            .ToDictionaryAsync(x => x.BlogId, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Page<Post>>> GetPostsByBlogIdAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        PagingArguments arguments,
        CancellationToken cancellationToken) =>
        await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .OrderBy(x => x.PostId)
            .ToBatchPageAsync(x => x.BlogId, arguments, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Page<Post>>> GetPostsByBlogIdProjectedAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        PagingArguments arguments,
        ISelectorBuilder selector,
        CancellationToken cancellationToken) =>
        await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .Select(selector.TryCompile<Post>()!)
            .OrderBy(x => x.PostId)
            .ToBatchPageAsync(x => x.BlogId, arguments, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Page<Post>>> GetPostsByBlogIdWithFilterAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        PagingArguments arguments,
        [DataLoaderState("where")] Expression<Func<Post, bool>> where,
        CancellationToken cancellationToken) =>
        await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .Where(where)
            .OrderBy(x => x.PostId)
            .ToBatchPageAsync(x => x.BlogId, arguments, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<int, Page<Post>>> GetPostsByBlogIdWithFilterProjectedAsync(
        IReadOnlyList<int> keys,
        BloggingContext context,
        PagingArguments arguments,
        [DataLoaderState("where")] Expression<Func<Post, bool>> where,
        ISelectorBuilder selector,
        CancellationToken cancellationToken) =>
        await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .Where(where)
            .Select(selector.TryCompile<Post>()!)
            .OrderBy(x => x.PostId)
            .ToBatchPageAsync(x => x.BlogId, arguments, cancellationToken);

    [DataLoader]
    public static async Task<ILookup<int, Post>> GetPostsFiltered(
        IReadOnlyList<int> keys,
        BloggingContext context,
        [DataLoaderState("where")] Expression<Func<Post, bool>> where,
        CancellationToken cancellationToken)
    {
        var result = await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .Where(where)
            .OrderBy(x => x.PostId)
            .ToArrayAsync(cancellationToken);

        return result.ToLookup(x => x.BlogId);
    }
}

// public class PersonDataLoaders
// {
//     [DataLoader]
//     public static async Task<Dictionary<Guid, Page<Client>>> PageClientsByApiIdWithFilterAsync(
//         IReadOnlyList<Guid> keys,
//         ApiContext context,
//         PagingArguments arguments,
//         [DataLoaderState("where")] Expression<Func<Client, bool>> where,
//         CancellationToken cancellationToken)
//     {
//         return await context.Clients
//             .Where(x => keys.Contains(x.ApiId))
//             .Where(where)
//             .OrderBy(x => x.Name)
//             .ThenBy(x => x.Id)
//             .ToBatchPageAsync(x => x.ApiId, arguments, cancellationToken);
//     }
// }
// }
/**
        var res = await clients.PageClientsByApiIdWithFilter
            .Where(x => x.Api!.Workspace.Members.Any(y => y.UserId == Guid.Empty))
            .WithPagingArguments(new PagingArguments(0))
            .LoadAsync(Guid.Empty, default);
*/