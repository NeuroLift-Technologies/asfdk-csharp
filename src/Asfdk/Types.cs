namespace Asfdk;

public enum FoundationMode { Unified, CrisisOnly, ContinuityOnly, FrameworkOnly, Development }
public enum InteractionType { EmotionalAssessment, CrisisAlert, PreferenceUpdate, OptimizationRequest, StatusInquiry, EmergencyEscalation }
public enum Channel { UserInput, ModelOutput, ToolResult, System, Unknown }
public enum RiskLevel { Low, Medium, High }
public enum SecurityEventType { InjectionAttempt, ValidationFailure, LengthExceeded }
public enum OutputSchemaType { Json, Text }
public enum CrisisLevel { Green, Yellow, Orange, Red, Black }

public static class EnumNames
{
    private static readonly Dictionary<FoundationMode, string> Fm = new()
    {
        [FoundationMode.Unified] = "unified",
        [FoundationMode.CrisisOnly] = "crisis_only",
        [FoundationMode.ContinuityOnly] = "continuity",
        [FoundationMode.FrameworkOnly] = "framework",
        [FoundationMode.Development] = "development"
    };
    private static readonly Dictionary<InteractionType, string> It = new()
    {
        [InteractionType.EmotionalAssessment] = "emotional_assessment",
        [InteractionType.CrisisAlert] = "crisis_alert",
        [InteractionType.PreferenceUpdate] = "preference_update",
        [InteractionType.OptimizationRequest] = "optimization_request",
        [InteractionType.StatusInquiry] = "status_inquiry",
        [InteractionType.EmergencyEscalation] = "emergency_escalation"
    };
    private static readonly Dictionary<Channel, string> Ch = new()
    {
        [Channel.UserInput] = "user_input",
        [Channel.ModelOutput] = "model_output",
        [Channel.ToolResult] = "tool_result",
        [Channel.System] = "system",
        [Channel.Unknown] = "unknown"
    };
    public static string Name(this FoundationMode m) => Fm[m];
    public static string Name(this InteractionType t) => It[t];
    public static string Name(this Channel c) => Ch[c];
    public static Channel ParseChannel(string? s)
    {
        if (string.IsNullOrEmpty(s)) return Channel.Unknown;
        foreach (var kvp in Ch)
        {
            if (kvp.Value == s) return kvp.Key;
        }
        return Channel.Unknown;
    }
    public static FoundationMode ParseMode(string? s)
    {
        if (string.IsNullOrEmpty(s)) return FoundationMode.Unified;
        foreach (var kvp in Fm)
        {
            if (kvp.Value == s) return kvp.Key;
        }
        return FoundationMode.Unified;
    }
}

public static class ChannelNormalizer
{
    public static Channel Normalize(object? value)
    {
        if (value is string s) return EnumNames.ParseChannel(s);
        return Channel.Unknown;
    }
}

public class FoundationComponents
{
    public bool? ToiOtoiFramework { get; set; }
    public bool? SleepwalkerProtocol { get; set; }
    public bool? RrtAdvocate { get; set; }
}

public class FoundationConfig
{
    public string UserId { get; set; } = "";
    public FoundationMode Mode { get; set; } = FoundationMode.Unified;
    public FoundationComponents? Components { get; set; }
    public object? Toi { get; set; }
}

public class UserInteraction
{
    public DateTime Timestamp { get; set; }
    public InteractionType InteractionType { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string UserId { get; set; } = "";
    public string? SessionId { get; set; }
    public int? Priority { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public Channel? Channel { get; set; }
}

public class FoundationResponse
{
    public DateTime Timestamp { get; set; }
    public string ResponseType { get; set; } = "";
    public Dictionary<string, object> Content { get; set; } = new();
    public List<string> ComponentsInvolved { get; set; } = new();
    public bool Success { get; set; }
}

public class ComponentStatus
{
    public bool Active { get; set; }
    public string Mode { get; set; } = "";
    public string? Error { get; set; }
}

public class HealthCheckResult
{
    public bool Healthy { get; set; }
    public Dictionary<string, ComponentStatus> Components { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class SanitizationResult
{
    public bool Clean { get; set; }
    public string Content { get; set; } = "";
    public string? Reason { get; set; }
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
}

public class ValidationResult
{
    public bool Valid { get; set; }
    public string? Reason { get; set; }
}

public class SecurityEvent
{
    public SecurityEventType EventType { get; set; }
    public string UserId { get; set; } = "";
    public string Details { get; set; } = "";
    public long Timestamp { get; set; }
}

public class ValidationIssue
{
    public string Message { get; set; } = "";
    public string Path { get; set; } = "";
    public string Code { get; set; } = "";
}

public class TOIValidationResult
{
    public bool Valid { get; set; }
    public List<ValidationIssue>? Errors { get; set; }
    public System.Text.Json.JsonElement? Toi { get; set; }
}

public class OTOIValidationResult
{
    public bool Valid { get; set; }
    public List<ValidationIssue>? Errors { get; set; }
    public System.Text.Json.JsonElement? Charter { get; set; }
}

public class EmotionalState
{
    public string State { get; set; } = "";
    public float Confidence { get; set; }
    public List<string> Indicators { get; set; } = new();
    public Dictionary<string, float> RawScores { get; set; } = new();
}

public class EmotionalStateWithProvenance : EmotionalState
{
    public Channel Channel { get; set; } = Channel.Unknown;
    public bool Trusted { get; set; }
    public bool? Flagged { get; set; }
    public string? FlagReason { get; set; }
}

public class CrisisAssessment
{
    public DateTime Timestamp { get; set; }
    public CrisisLevel CrisisLevel { get; set; }
    public List<string> PrimaryIndicators { get; set; } = new();
    public List<string> SecondaryIndicators { get; set; } = new();
    public float ConfidenceScore { get; set; }
    public float? EstimatedDuration { get; set; }
    public List<string> RecommendedInterventions { get; set; } = new();
    public float EscalationThreshold { get; set; }
    public float UserSafetyScore { get; set; }
    public Dictionary<string, object> ContextFactors { get; set; } = new();
}

public class CrisisAssessmentWithProvenance : CrisisAssessment
{
    public Channel Channel { get; set; } = Channel.Unknown;
    public bool Trusted { get; set; }
    public bool? Flagged { get; set; }
    public string? FlagReason { get; set; }
}
