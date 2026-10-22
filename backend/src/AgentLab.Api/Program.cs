DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://localhost:5080");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5176")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("ReactDev");
app.MapOpenApi();

app.MapGet("/api/health", () =>
    Results.Ok(new { status = "healthy" }));

app.MapPost("/api/chat", (ChatRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest(
            new { error = "Message is required." });

    return Results.Ok(new ChatResponse(
        $"AgentLab received: {request.Message}. " +
        "Our AI agent will be connected in Phase 2."
    ));
});

app.Run();

public record ChatRequest(string Message);
public record ChatResponse(string Reply);