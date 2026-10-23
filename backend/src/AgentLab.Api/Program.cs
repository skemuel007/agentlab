using AgentLab.Api.Agents;

var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
if (!File.Exists(envFile))
    envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
    DotNetEnv.Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://localhost:5080");

builder.Services.AddSingleton<AgentService>();

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

app.UseCors("ReactDev");
app.MapOpenApi();

app.MapGet("/api/health", () =>
    Results.Ok(new { status = "healthy" }));

app.MapPost("/api/chat", async (ChatRequest request,
AgentService agentService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(
            new { error = "Message is required." });
    }

    try
    {
        var reply = await agentService.AskAsync(
            request.Message,
            cancellationToken);

        return Results.Ok(new ChatResponse(reply));
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

app.Run();

public record ChatRequest(string Message);
public record ChatResponse(string Reply);