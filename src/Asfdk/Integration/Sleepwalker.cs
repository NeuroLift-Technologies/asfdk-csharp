namespace Asfdk;

public static class Sleepwalker
{
    public static EmotionalStateWithProvenance DetectEmotionalState(
        string userInput,
        List<object>? sessionHistory = null,
        Channel? channel = null,
        string userId = "unknown")
    {
        var resolved = ChannelNormalizer.Normalize(channel);
        var sanitizationResult = PromptDefense.SanitizeInput(userInput);
        var flagged = !sanitizationResult.Clean;

        if (flagged)
        {
            PromptDefense.LogSecurityEvent(new SecurityEvent
            {
                EventType = sanitizationResult.RiskLevel == RiskLevel.High
                    ? SecurityEventType.InjectionAttempt
                    : SecurityEventType.ValidationFailure,
                UserId = userId,
                Details = sanitizationResult.Reason ?? "Input sanitization flagged in Sleepwalker assessment",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        var state = AnalyzeEmotionalState(sanitizationResult.Content, sessionHistory ?? new List<object>());

        var result = new EmotionalStateWithProvenance
        {
            State = state.State,
            Confidence = state.Confidence,
            Indicators = state.Indicators,
            RawScores = state.RawScores,
            Channel = resolved,
            Trusted = resolved == Channel.UserInput
        };

        if (flagged)
        {
            result.Flagged = true;
            result.FlagReason = sanitizationResult.Reason;
        }

        return result;
    }

    public static object AssessInteraction(
        string userInput,
        List<object>? sessionHistory = null,
        Channel? channel = null,
        string userId = "unknown")
    {
        var resolved = ChannelNormalizer.Normalize(channel);
        var sanitizationResult = PromptDefense.SanitizeInput(userInput);
        var flagged = !sanitizationResult.Clean;

        if (flagged)
        {
            PromptDefense.LogSecurityEvent(new SecurityEvent
            {
                EventType = sanitizationResult.RiskLevel == RiskLevel.High
                    ? SecurityEventType.InjectionAttempt
                    : SecurityEventType.ValidationFailure,
                UserId = userId,
                Details = sanitizationResult.Reason ?? "Input sanitization flagged in Sleepwalker assessment",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        var result = AnalyzeInteraction(sanitizationResult.Content, sessionHistory ?? new List<object>());

        var resultDict = new Dictionary<string, object>
        {
            ["channel"] = resolved,
            ["trusted"] = resolved == Channel.UserInput
        };

        if (flagged)
        {
            resultDict["flagged"] = true;
            resultDict["flag_reason"] = sanitizationResult.Reason ?? string.Empty;
        }

        foreach (var kvp in result)
        {
            resultDict[kvp.Key] = kvp.Value;
        }

        return resultDict;
    }

    public static bool RequiresRrtaHandoff(EmotionalState state)
    {
        return state.State?.ToLower() switch
        {
            "depressed" or "anxious" or "angry" or "distressed" => true,
            _ => state.Confidence > 0.7f && state.Indicators.Any(i =>
                i.Contains("suicidal", StringComparison.OrdinalIgnoreCase) ||
                i.Contains("self-harm", StringComparison.OrdinalIgnoreCase))
        };
    }

    public static ComponentStatus GetStatus()
    {
        return new ComponentStatus { Active = true, Mode = "emotional-continuity" };
    }

    public static void Reset()
    {
    }

    private static EmotionalState AnalyzeEmotionalState(string input, List<object> sessionHistory)
    {
        var lowerInput = input.ToLower();
        var indicators = new List<string>();
        var scores = new Dictionary<string, float>();

        if (lowerInput.Contains("sad") || lowerInput.Contains("depressed"))
        {
            indicators.Add("depression_keywords");
            scores["depression"] = 0.7f;
        }
        if (lowerInput.Contains("anxious") || lowerInput.Contains("anxiety"))
        {
            indicators.Add("anxiety_keywords");
            scores["anxiety"] = 0.6f;
        }
        if (lowerInput.Contains("angry") || lowerInput.Contains("anger"))
        {
            indicators.Add("anger_keywords");
            scores["anger"] = 0.5f;
        }
        if (lowerInput.Contains("happy") || lowerInput.Contains("joy"))
        {
            scores["happiness"] = 0.8f;
        }

        var state = indicators.Any() ? "distressed" : "neutral";
        var confidence = indicators.Any() ? 0.7f : 0.3f;

        return new EmotionalState
        {
            State = state,
            Confidence = confidence,
            Indicators = indicators,
            RawScores = scores
        };
    }

    private static Dictionary<string, object> AnalyzeInteraction(string input, List<object> sessionHistory)
    {
        var emotionalState = AnalyzeEmotionalState(input, sessionHistory);

        return new Dictionary<string, object>
        {
            ["emotionalState"] = emotionalState,
            ["explicitSuicidalIdeation"] = false,
            ["selfHarmIndicators"] = false,
            ["inabilityToEnsureSafety"] = false
        };
    }
}
