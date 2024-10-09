using System;
using System.Threading.Tasks;
using DataLoader.State.Example;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<BloggingContext>("db");
builder.Services.AddCors(x =>
    x.AddDefaultPolicy(y => y.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services
    .AddGraphQLServer()
    .AddPagingArguments()
    .AddExampleTypes()
    .ModifyRequestOptions(x => x.IncludeExceptionDetails = true);

var app = builder.Build();

await app.Seed();

app.UseCors();
app.MapGraphQL();

await app.RunAsync();

public static class ApplicationBuilderExtensions
{
    public static async Task Seed(this WebApplication app)
    {
        bool error = false;
        int retries = 0;
        START:
        try
        {
            error = false;
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<BloggingContext>();
            await context.Database.EnsureCreatedAsync();

            var hasBlogs = await context.Blogs.AnyAsync();
            if (!hasBlogs)
            {
                var blogs = new[]
                {
                    new Blog { BlogId = 1, Title = "GraphQL Rocks" },
                    new Blog { BlogId = 2, Title = "Hot Chocolate" },
                    new Blog { BlogId = 3, Title = "Green Donut" },
                    new Blog { BlogId = 4, Title = "DataLoader" },
                    new Blog { BlogId = 5, Title = "Entity Framework Core" }
                };
                await context.Blogs.AddRangeAsync(blogs);
                await context.SaveChangesAsync();
            }

            var hasPosts = await context.Posts.AnyAsync();
            if (!hasPosts)
            {
                var posts = new[]
                {
                    new Post
                    {
                        PostId = 1, BlogId = 1, Title = "GraphQL Rocks",
                        Content = "GraphQL is awesome!"
                    },
                    new Post
                    {
                        PostId = 2, BlogId = 1, Title = "Hot Chocolate",
                        Content = "Hot Chocolate is a GraphQL server implementation for .NET"
                    },
                    new Post
                    {
                        PostId = 3, BlogId = 2, Title = "Green Donut",
                        Content = "Green Donut is a data loader for .NET"
                    },
                    new Post
                    {
                        PostId = 4, BlogId = 2, Title = "DataLoader",
                        Content =
                            "DataLoader is a generic utility to be used as part of your application's data fetching layer"
                    },
                    new Post
                    {
                        PostId = 5, BlogId = 3, Title = "Entity Framework Core",
                        Content =
                            "Entity Framework Core is a modern object-database mapper for .NET"
                    }
                };
                await context.Posts.AddRangeAsync(posts);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            error = true;
            Console.WriteLine(ex.Message);
        }

        if (error && retries++ < 30)
        {
            Console.WriteLine("Retrying in 1 second...");
            await Task.Delay(1000);
            goto START;
        }
    }
}