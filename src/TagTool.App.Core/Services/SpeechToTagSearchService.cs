using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

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

    public SpeechToTagSearchService(ILogger<SpeechToTagSearchService> logger)
    {
        _logger = logger;
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

    private Task<string?> GetSpeechTranscription(string filePath)
    {
        _logger.LogWarning("This feature is not implemented!");
        return Task.FromResult<string?>(null);
    }

    private Task<IEnumerable<string>?> TranscriptionToTags(string transcription)
    {
        throw new NotImplementedException();
    }
}
