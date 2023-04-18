using System;
using Newtonsoft.Json;
using Spectre.Console;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatBot
{
    class Program
    {
        const string key = "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";
        const string url = "https://api.openai.com/v1/chat/completions";
        static void Main(string[] args)
        {
            // Chat(args).GetAwaiter().GetResult();
            Task.Run(async () =>
            {
                var messages = new List<dynamic>
                {
                    new {role = "system",
                        content = "You are ChatGPT, a music composer " +
                                                    "model trained by OpenAI. " +
                                                    "Answer as concisely as possible.  " +
                                                    "Make a joke every few lines just to spice things up."},
                    new {role = "assistant",
                        content = "How can I help you?"}
                 };
                AnsiConsole.MarkupLine($"[purple]MACHINE:[/] [blue]{Markup.Escape(messages[1].content)}[/]");
                while (true)
                {
                    // Capture the users messages and add to
                    // messages list for submitting to the chat API
                    var userMessage = AnsiConsole.Ask<string>($"[purple]USER:[/] ");
                    messages.Add(new { role = "user", content = userMessage });

                    // Create the request for the API sending the
                    // latest collection of chat messages
                    var request = new
                    {
                        messages,
                        model = "gpt-3.5-turbo",
                        max_tokens = 300,

                    };

                    // Send the request and capture the response
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
                    var requestJson = JsonConvert.SerializeObject(request);
                    var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
                    var httpResponseMessage = await httpClient.PostAsync(url, requestContent);
                    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
                    {
                        choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                        error = new { message = string.Empty }
                    });


                    if (!string.IsNullOrEmpty(responseObject?.error?.message))  // Check for errors
                    {
                        AnsiConsole.MarkupLine($"[bold red]{Markup.Escape(responseObject?.error.message)}[/]");
                    }
                    else  // Add the message object to the message collection
                    {
                        var messageObject = responseObject?.choices[0].message;
                        messages.Add(messageObject);
                        AnsiConsole.MarkupLine($"[purple]MACHINE:[/] [blue]{Markup.Escape(messageObject.content)}[/]");
                    }
                }
            }).GetAwaiter().GetResult();
           
        }
    }
}
