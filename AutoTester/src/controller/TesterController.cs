using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AiUsabilityTester;
using AutoTester.domain;
using AutoTester.util;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;

namespace AutoTester.controller;

public class TesterController : ControllerBase
{
    private static Dictionary<string, UsabilityTester> _testerRegistry =
        new Dictionary<string, UsabilityTester>();
    
    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(
                       new WebSocketAcceptContext { DangerousEnableCompression = true }))
            {
                await Handshake(webSocket);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    private async Task Handshake(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (!receiveResult.CloseStatus.HasValue)
        {
            // https://github.com/Azure-Samples/openai-dotnet-samples
            var azureOpenAiKey = Environment.GetEnvironmentVariable("AOAI_KEY");
            var openAiClient = new OpenAIClient(azureOpenAiKey);
            
            string task = Encoding.ASCII.GetString(buffer, 0, receiveResult.Count);

            var testingTask = JsonSerializer.Deserialize<TestingTask>(task);

            await ExecuteTestingTask(testingTask, openAiClient);
            
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private async Task<UsabilityTester> CreateTester()
    {
        var guid = Guid.NewGuid();
        var tester = new UsabilityTester(guid);
        
        using var playwright = await Playwright.CreateAsync();

        // Launch a browser (e.g., Chromium)
        await using var browser = await playwright.Chromium.LaunchAsync();

        tester.playwright = playwright;
        tester.browser = browser;
        
        _testerRegistry[guid.ToString()] = tester;
        
        return tester;
    }

    private async Task<IBrowser> ExecuteTestingTask(TestingTask? task, OpenAIClient openAiClient)
    {
        if (task == null)
        {
            return null;
        }
        
        // Send Async request to openAI for text and/or screenshot
        var tester = await CreateTester();
        
        // Parse json, extract user interaction commands and more
        var browser = tester.browser;

        // Create a new page
        var page = await browser.NewPageAsync();

        // Navigate to a website
        await page.GotoAsync(task.website);
        
        var options = new PageScreenshotOptions();

        var currentPageImage = BinaryData.FromBytes(await page.ScreenshotAsync(options));

        ChatCompletionsOptions ccOptions = new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-4o", // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models
            Messages = {
                new ChatRequestSystemMessage(OpenAIUtils.SYSTEM_ROLE),
                new ChatRequestUserMessage {
                    MultimodalContentItems =
                    {
                        new ChatMessageTextContentItem(task.task),
                        new ChatMessageImageContentItem(currentPageImage, "image/png", ChatMessageImageDetailLevel.Auto)
                    }
                },
            },
            MaxTokens = 250
        };
        
        do
        {
            var completionsResponse = await openAiClient.GetChatCompletionsAsync(ccOptions, CancellationToken.None);
            var completions = completionsResponse.Value;
            var choices = completions.Choices;

            if (choices.Count > 0)
            {
                var content = choices[0].Message.Content;
                
                var nextInteractions = JsonSerializer.Deserialize<NextInteractions>(content);

                if (nextInteractions != null)
                {
                    // Perform playwright operations
                    PlayWrightUtils.executeInteractions(nextInteractions, page);
                    
                    // Check if the task is completed
                    if (nextInteractions.completed)
                    {
                        break;
                    }
                    
                    ccOptions.Messages.Add(new ChatRequestAssistantMessage(content));
                    ccOptions.Messages.Add(new ChatRequestUserMessage {
                        MultimodalContentItems =
                        {
                            new ChatMessageImageContentItem(currentPageImage, "image/png", ChatMessageImageDetailLevel.Auto)
                        }
                    });
                }
            }
        } 
        while (true);
        
        // Close the browser
        await browser.CloseAsync();

        return browser;
    }
}