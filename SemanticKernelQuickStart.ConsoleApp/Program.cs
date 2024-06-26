// Import packages
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create a kernel with Azure OpenAI chat completion
//read endpoint and apiKey from environment variables
var endpoint = Environment.GetEnvironmentVariable("aoai-sk-endpoint");
var apiKey = Environment.GetEnvironmentVariable("aoai-sk-api-key");
//Check if the endpoint and apiKey are not null
if (endpoint is null || apiKey is null)
{
    Console.WriteLine("Please set the environment variables aoai-sk-endpoint and aoai-sk-api");
    return;
}
        
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion("nova-4", endpoint, apiKey);


// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();
    // If the user input is null, break the loop
    if (userInput is null)
    {
        break;
    }

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);