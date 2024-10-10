using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using GreenDonut.Internals;
using HotChocolate.Pagination;

namespace DataLoader.State.Example;

public static class WhereDataLoaderExtensions
{
    public static IDataLoader<TKey, TValue> Where<TKey, TValue>(
        this IDataLoader<TKey, TValue> dataLoader,
        Expression<Func<TValue, bool>>? where)
        where TKey : notnull
    {
        if (dataLoader is null)
        {
            throw new ArgumentNullException(nameof(dataLoader));
        }

        if (where is null)
        {
            return dataLoader;
        }

        var branchKey = where.ToString();
        return (IDataLoader<TKey, TValue>)dataLoader.Branch(branchKey, CreateBranch, where);

        static IDataLoader CreateBranch(
            string key,
            IDataLoader<TKey, TValue> dataLoader,
            Expression<Func<TValue, bool>> selector)
        {
            var branch = new WhereDataLoader<TKey, TValue>(
                (DataLoaderBase<TKey, TValue>)dataLoader,
                key);
            branch.ContextData = branch.ContextData.SetItem("where", selector);

            return branch;
        }
    }

    public static IDataLoader<TKey, Page<TValue>> Where<TKey, TValue>(
        this IDataLoader<TKey, Page<TValue>> dataLoader,
        Expression<Func<TValue, bool>>? where)
        where TKey : notnull
    {
        if (dataLoader is null)
        {
            throw new ArgumentNullException(nameof(dataLoader));
        }

        if (where is null)
        {
            return dataLoader;
        }

        var branchKey = where.ToString();
        return (IDataLoader<TKey, Page<TValue>>)dataLoader.Branch(branchKey, CreateBranch, where);

        static IDataLoader CreateBranch(
            string key,
            IDataLoader<TKey, Page<TValue>> dataLoader,
            Expression<Func<TValue, bool>> selector)
        {
            var branch = new WhereDataLoader<TKey, Page<TValue>>(
                (DataLoaderBase<TKey, Page<TValue>>)dataLoader,
                key);
            branch.ContextData = branch.ContextData.SetItem("where", selector);

            return branch;
        }
    }

    public static IDataLoader<TKey, TValue[]> Where<TKey, TValue>(
        this IDataLoader<TKey, TValue[]> dataLoader,
        Expression<Func<TValue, bool>>? where)
        where TKey : notnull
    {
        if (dataLoader is null)
        {
            throw new ArgumentNullException(nameof(dataLoader));
        }

        if (where is null)
        {
            return dataLoader;
        }

        var branchKey = where.ToString();
        return (IDataLoader<TKey, TValue[]>)dataLoader.Branch(branchKey, CreateBranch, where);

        static IDataLoader CreateBranch(
            string key,
            IDataLoader<TKey, TValue[]> dataLoader,
            Expression<Func<TValue, bool>> selector)
        {
            var branch = new WhereDataLoader<TKey, TValue[]>(
                (DataLoaderBase<TKey, TValue[]>)dataLoader,
                key);
            branch.ContextData = branch.ContextData.SetItem("where", selector);

            return branch;
        }
    }
}

public sealed class WhereDataLoader<TKey, TValue>(
    DataLoaderBase<TKey, TValue> root,
    string whereKey)
    : DataLoaderBase<TKey, TValue>(DataLoaderHelper.GetBatchScheduler(root),
        DataLoaderHelper.GetOptions(root))
    where TKey : notnull
{
    protected override ValueTask FetchAsync(
        IReadOnlyList<TKey> keys,
        Memory<Result<TValue?>> results,
        DataLoaderFetchContext<TValue> context,
        CancellationToken cancellationToken)
        => DataLoaderHelper.FetchAsync(root, keys, results, context, cancellationToken);

    protected override string CacheKeyType { get; } =
        $"{DataLoaderHelper.GetCacheKeyType(root)}:{whereKey}";

    protected override bool AllowCachePropagation => false;

    protected override bool AllowBranching => true;
}