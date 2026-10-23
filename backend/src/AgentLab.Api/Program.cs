using System.Diagnostics;
using AgentLab.Api.Agents;

var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
if (!File.Exists(envFile))
    envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
    DotNetEnv.Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://localhost:5080");

builder.Services.AddSingleton<IAgentService, AgentService>();

// get the allowed origins from the environment variable
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();

    app.Logger.LogInformation(
        "HTTP {Method} {Path} started.",
        context.Request.Method,
        context.Request.Path);

    try
    {
        await next(context);
    }
    catch (Exception exception)
    {
        app.Logger.LogError(
            exception,
            "HTTP {Method} {Path} failed after {ElapsedMilliseconds} ms.",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds);

        throw;
    }
    finally
    {
        stopwatch.Stop();

        app.Logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms.",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});

app.UseCors("ReactDev");
app.MapOpenApi();

app.MapGet("/api/health", () =>
    Results.Ok(new { status = "healthy" }));

app.MapPost("/api/chat", async (ChatRequest request,
IAgentService agentService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(
            new { error = "Message is required." });
    }

    try
    {
        app.Logger.LogInformation("Processing chat request.");

        var reply = await agentService.AskAsync(
            request.Message,
            cancellationToken);

        app.Logger.LogInformation("Chat request completed successfully.");

        return Results.Ok(new ChatResponse(agentService.AgentId, reply));
    }
    catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
    {
        return Results.StatusCode(499);
    }
    catch (Exception exception)
    {
        app.Logger.LogError(
            exception, "Agent execution failed.");

        return Results.Problem(
            title: "Agent execution failed",
            detail: "Unable to generate a response.",
            statusCode: 502);
    }
});

app.Logger.LogInformation("AgentLab API starting on http://localhost:5080.");

app.Run();

public record ChatRequest(string Message);
public record ChatResponse(string AgentId, string Reply);

public partial class Program;
