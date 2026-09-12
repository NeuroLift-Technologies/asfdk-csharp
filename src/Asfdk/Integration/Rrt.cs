namespace Asfdk;

public static class Rrt
{
    private static readonly Dictionary<string, CrisisEngine> _engines = new();

    public static async Task<CrisisAssessmentWithProvenance> Assess(
        string userId,
        string inputText,
        Channel? channel = null)
    {
        var resolved = ChannelNormalizer.Normalize(channel);
        var sanitizationResult = PromptDefense.SanitizeInput(inputText);
        var flagged = !sanitizationResult.Clean;

        if (flagged)
        {
            PromptDefense.LogSecurityEvent(new SecurityEvent
            {
                EventType = sanitizationResult.RiskLevel == RiskLevel.High
                    ? SecurityEventType.InjectionAttempt
                    : SecurityEventType.ValidationFailure,
                UserId = userId,
                Details = sanitizationResult.Reason ?? "Input sanitization flagged in RRT assessment",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        var assessment = await GetEngine(userId).AssessAsync(sanitizationResult.Content);

        var result = new CrisisAssessmentWithProvenance
        {
            Timestamp = assessment.Timestamp,
            CrisisLevel = assessment.CrisisLevel,
            PrimaryIndicators = assessment.PrimaryIndicators,
            SecondaryIndicators = assessment.SecondaryIndicators,
            ConfidenceScore = assessment.ConfidenceScore,
            EstimatedDuration = assessment.EstimatedDuration,
            RecommendedInterventions = assessment.RecommendedInterventions,
            EscalationThreshold = assessment.EscalationThreshold,
            UserSafetyScore = assessment.UserSafetyScore,
            ContextFactors = assessment.ContextFactors,
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

    public static ComponentStatus GetStatus()
    {
        return new ComponentStatus { Active = true, Mode = "crisis-detection" };
    }

    public static void Reset(string? userId = null)
    {
        if (userId == null)
        {
            _engines.Clear();
        }
        else
        {
            _engines.Remove(userId);
        }
    }

    public static void ResetSession(string? userId = null)
    {
        if (userId == null)
        {
            Reset();
            return;
        }

        if (_engines.TryGetValue(userId, out var engine))
        {
            engine.ResetSession();
        }
    }

    private static CrisisEngine GetEngine(string userId)
    {
        if (!_engines.ContainsKey(userId))
        {
            _engines[userId] = new CrisisEngine(userId);
        }
        return _engines[userId];
    }
}

public class CrisisEngine
{
    private readonly string _userId;
    private readonly Dictionary<string, object> _sessionState = new();

    public CrisisEngine(string userId)
    {
        _userId = userId;
    }

    public async Task<CrisisAssessment> AssessAsync(string input)
    {
        return await Task.Run(() => Assess(input));
    }

    private CrisisAssessment Assess(string input)
    {
        var lowerInput = input.ToLower();
        var primaryIndicators = new List<string>();
        var secondaryIndicators = new List<string>();
        var contextFactors = new Dictionary<string, object>();

        var crisisLevel = CrisisLevel.Green;
        var safetyScore = 0.9f;

        if (lowerInput.Contains("suicidal") || lowerInput.Contains("kill myself") || lowerInput.Contains("end my life"))
        {
            primaryIndicators.Add("suicidal_ideation");
            crisisLevel = CrisisLevel.Black;
            safetyScore = 0.1f;
        }
        else if (lowerInput.Contains("self-harm") || lowerInput.Contains("hurt myself"))
        {
            primaryIndicators.Add("self_harm");
            crisisLevel = CrisisLevel.Red;
            safetyScore = 0.3f;
        }
        else if (lowerInput.Contains("hopeless") || lowerInput.Contains("can't go on") || lowerInput.Contains("give up"))
        {
            secondaryIndicators.Add("hopelessness");
            if (crisisLevel == CrisisLevel.Green)
            {
                crisisLevel = CrisisLevel.Orange;
                safetyScore = 0.5f;
            }
        }
        else if (lowerInput.Contains("anxious") || lowerInput.Contains("panic") || lowerInput.Contains("overwhelmed"))
        {
            secondaryIndicators.Add("anxiety");
            if (crisisLevel == CrisisLevel.Green)
            {
                crisisLevel = CrisisLevel.Yellow;
                safetyScore = 0.7f;
            }
        }

        var interventions = new List<string>();
        if (crisisLevel >= CrisisLevel.Red)
        {
            interventions.Add("Seek immediate professional help");
            interventions.Add("Contact 988 Suicide & Crisis Lifeline");
        }
        else if (crisisLevel >= CrisisLevel.Orange)
        {
            interventions.Add("Consider reaching out to a trusted person");
            interventions.Add("Monitor for escalating risk");
        }

        return new CrisisAssessment
        {
            Timestamp = DateTime.UtcNow,
            CrisisLevel = crisisLevel,
            PrimaryIndicators = primaryIndicators,
            SecondaryIndicators = secondaryIndicators,
            ConfidenceScore = 0.8f,
            RecommendedInterventions = interventions,
            EscalationThreshold = 0.7f,
            UserSafetyScore = safetyScore,
            ContextFactors = contextFactors
        };
    }

    public void ResetSession()
    {
        _sessionState.Clear();
    }
}
