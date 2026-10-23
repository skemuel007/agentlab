using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

namespace AgentLab.Api.Agents;

public interface IAgentService
{
    string AgentId { get; }

    Task<string> AskAsync(string message, CancellationToken cancellationToken = default);
}

public sealed class AgentService : IAgentService
{
    public const string DefaultAgentId = "agentlab-assistant";

    private readonly AIAgent _agent;
    private readonly ILogger<AgentService> _logger;

    public string AgentId => DefaultAgentId;

    public AgentService(
        IConfiguration configuration,
        ILogger<AgentService> logger)
    {
        _logger = logger;

        var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
            ?? throw new InvalidOperationException(
                "AZURE_OPENAI_ENDPOINT is missing.");

        var deployment = configuration["AZURE_OPENAI_DEPLOYMENT"]
            ?? throw new InvalidOperationException(
                "AZURE_OPENAI_DEPLOYMENT is missing.");

        _logger.LogInformation(
            "Initializing AgentLab agent for deployment {Deployment}.",
            deployment);

        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new DefaultAzureCredential());

        _agent = client.GetChatClient(deployment)
            .AsAIAgent(
                name: AgentId,
                instructions: """
                    You are AgentLab Assistant.

                    Your role is to help software engineers
                    understand artificial intelligence,
                    agentic systems and software architecture.

                    Guidelines:
                    - Explain concepts clearly.
                    - Prefer C# and .NET examples.
                    - Provide practical explanations.
                    - Be honest when uncertain.
                    - Never invent sources or tool results.
                    """);

        _logger.LogInformation("AgentLab agent initialized successfully.");
    }

    public async Task<string> AskAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Agent run started with a {MessageLength}-character message.",
            message.Length);

        try
        {
            var response = await _agent.RunAsync(
                message,
                cancellationToken: cancellationToken);

            var reply = response.ToString();

            _logger.LogInformation(
                "Agent run completed in {ElapsedMilliseconds} ms with a {ReplyLength}-character reply.",
                stopwatch.ElapsedMilliseconds,
                reply.Length);

            return reply;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Agent run was cancelled after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Agent run failed after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

}
