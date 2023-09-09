using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace TagTool.App.Core.Services;

public interface ISpeechToTagSearchService
{
    Task<IEnumerable<string>> GetTranscriptionWords(string? filePath);

    Task<IEnumerable<string>> GetTags(string? filePath);
}

[UsedImplicitly]
public class SpeechToTagSearchService : ISpeechToTagSearchService
{
    private readonly ILogger<SpeechToTagSearchService> _logger;
    private readonly IOpenAIService _openAiService;

    public SpeechToTagSearchService(ILogger<SpeechToTagSearchService> logger, IOpenAIService openAiService)
    {
        _logger = logger;
        _openAiService = openAiService;
    }

    public async Task<IEnumerable<string>> GetTranscriptionWords(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return Enumerable.Empty<string>();
        }

        var transcription = await GetSpeechTranscription(filePath);

        if (transcription is null)
        {
            return Enumerable.Empty<string>();
        }

        char[] delimiterChars = { ' ', ',', '.', ':', '\t' };

        return transcription.Split(delimiterChars).Where(s => !string.IsNullOrWhiteSpace(s));
    }

    public async Task<IEnumerable<string>> GetTags(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return Enumerable.Empty<string>();
        }

        var transcription = await GetSpeechTranscription(filePath);

        if (transcription is null)
        {
            return Enumerable.Empty<string>();
        }

        var tagNames = await TranscriptionToTags(transcription);

        _logger.LogInformation("Tag name extracted from speech: {@TagNames}", tagNames);

        return tagNames ?? Enumerable.Empty<string>();
    }

    private async Task<string?> GetSpeechTranscription(string filePath)
    {
        // todo: prompt language should depend on user language
        var audioCreateTranscriptionRequest = new AudioCreateTranscriptionRequest
        {
            Model = OpenAI.GPT3.ObjectModels.Models.WhisperV1,
            File = await File.ReadAllBytesAsync(filePath),
            Prompt = "Voice command to search tag with names like NewTag, Year2022, Picture, HelloWorld",
            ResponseFormat = "text",
            FileName = "TagToSearchCommand.wav",
            Language = "en" // does not make things faster... 
        };

        _logger.LogInformation("Sending request {@AudioCreateTranscriptionRequest}", audioCreateTranscriptionRequest);
        var transcription = await _openAiService.Audio.CreateTranscription(audioCreateTranscriptionRequest, CancellationToken.None);

        if (transcription.Error is not null)
        {
            _logger.LogWarning("OpenAi replied with {Error} and {Text}", transcription.Error, transcription.Text);
            return null;
        }

        _logger.LogInformation("OpenAi replied with transcription: {Text}", transcription.Text);
        return transcription.Text.AsSpan().Trim('\n').Trim().ToString();
    }

    private async Task<IEnumerable<string>?> TranscriptionToTags(string transcription)
    {
        var completionsRequest = new CompletionCreateRequest
        {
            Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV3,
            Prompt = $"Extract tag names from the text: {transcription}. Create comma separated list from these tags",
            MaxTokens = 50
        };

        _logger.LogInformation("Sending request {@CompletionCreateRequest}", completionsRequest);
        var editCreateResponse = await _openAiService.Completions.CreateCompletion(completionsRequest);

        if (editCreateResponse.Error is not null)
        {
            _logger.LogWarning("OpenAi replied with {Error} and {@Choices}", editCreateResponse.Error, editCreateResponse.Choices);
            {
                return null;
            }
        }

        _logger.LogInformation("OpenAi replied with choices {@Choices}", editCreateResponse.Choices);

        return editCreateResponse.Choices.First().Text
            .AsSpan()
            .Trim(new[] { '\n', '[', ']', '(', ')' })
            .Trim()
            .ToString()
            .Split(',')
            .Select(s => s.Trim());
    }
}
