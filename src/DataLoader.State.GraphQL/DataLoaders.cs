using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using GreenDonut.Projections;
using HotChocolate.Data;
using HotChocolate.Data.Sorting;
using HotChocolate.Pagination;
using HotChocolate.Resolvers;
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
        [DataLoaderState("where")] Expression<Func<Post, bool>> where,
        ISelectorBuilder selector,
        CancellationToken cancellationToken) =>
        await context.Posts
            .Where(x => keys.Contains(x.BlogId))
            .Where(where)
            .OrderBy(x => x.PostId)
            .Select(x => x.BlogId, selector)
            .ToBatchPageAsync(x => x.BlogId, arguments, cancellationToken);
}