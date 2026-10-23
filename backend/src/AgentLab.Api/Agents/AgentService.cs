using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

namespace AgentLab.Api.Agents;

public sealed class AgentService
{
    private readonly AIAgent _agent;

    public AgentService(IConfiguration configuration)
    {
        var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
            ?? throw new InvalidOperationException(
                "AZURE_OPENAI_ENDPOINT is missing.");

        var deployment = configuration["AZURE_OPENAI_DEPLOYMENT"]
            ?? throw new InvalidOperationException(
                "AZURE_OPENAI_DEPLOYMENT is missing.");

        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new DefaultAzureCredential());

        _agent = client.GetChatClient(deployment)
            .AsAIAgent(
                name: "AgentLabAssistant",
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
    }

    public async Task<string> AskAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var response = await _agent.RunAsync(
            message,
            cancellationToken: cancellationToken);

        return response.ToString();
    }

}