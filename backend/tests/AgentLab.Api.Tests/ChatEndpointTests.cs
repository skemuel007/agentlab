using System.Net;
using System.Net.Http.Json;
using AgentLab.Api.Agents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AgentLab.Api.Tests;

public sealed class ChatEndpointTests
{
    [Fact]
    public async Task PostChat_ReturnsAgentIdentifierAndReply_FromMockedAgentService()
    {
        var mockAgentService = new MockAgentService(
            agentId: "test-agent",
            reply: "Mocked response");

        using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAgentService>();
                    services.AddSingleton<IAgentService>(mockAgentService);
                });
            });

        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/chat",
            new { message = "Hello" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ChatResponse>();

        Assert.NotNull(content);
        Assert.Equal("test-agent", content.AgentId);
        Assert.Equal("Mocked response", content.Reply);
        Assert.Equal(1, mockAgentService.CallCount);
        Assert.Equal("Hello", mockAgentService.LastMessage);
    }

    private sealed record ChatResponse(string AgentId, string Reply);

    private sealed class MockAgentService(
        string agentId,
        string reply) : IAgentService
    {
        public string AgentId { get; } = agentId;

        public int CallCount { get; private set; }

        public string? LastMessage { get; private set; }

        public Task<string> AskAsync(
            string message,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastMessage = message;

            return Task.FromResult(reply);
        }
    }
}
