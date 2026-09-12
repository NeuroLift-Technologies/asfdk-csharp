namespace Asfdk;

public class NeuroLiftFoundation
{
    private readonly FoundationConfig _config;
    private readonly ActiveComponents _active;
    private bool _initialized;
    private System.Text.Json.JsonElement? _toiDocument;

    public NeuroLiftFoundation(FoundationConfig config)
    {
        _config = config;
        _active = ComponentsForMode(config.Mode, config.Components);
    }

    private class ActiveComponents
    {
        public bool Toi { get; }
        public bool Swp { get; }
        public bool Rrt { get; }

        public ActiveComponents(bool toi, bool swp, bool rrt)
        {
            Toi = toi;
            Swp = swp;
            Rrt = rrt;
        }
    }

    private static ActiveComponents ComponentsForMode(FoundationMode mode, FoundationComponents? overrides)
    {
        var defaults = new Dictionary<FoundationMode, ActiveComponents>
        {
            [FoundationMode.Unified] = new ActiveComponents(true, true, true),
            [FoundationMode.CrisisOnly] = new ActiveComponents(false, false, true),
            [FoundationMode.ContinuityOnly] = new ActiveComponents(false, true, false),
            [FoundationMode.FrameworkOnly] = new ActiveComponents(true, false, false),
            [FoundationMode.Development] = new ActiveComponents(true, true, false),
        };

        var baseComponents = defaults.GetValueOrDefault(mode, new ActiveComponents(false, false, false));

        bool Pick(bool? overrideValue, bool fallback) => overrideValue ?? fallback;

        return new ActiveComponents(
            Pick(overrides?.ToiOtoiFramework, baseComponents.Toi),
            Pick(overrides?.SleepwalkerProtocol, baseComponents.Swp),
            Pick(overrides?.RrtAdvocate, baseComponents.Rrt)
        );
    }

    public async Task Initialize()
    {
        _toiDocument = GenerateToi();
        _initialized = true;
    }

    private System.Text.Json.JsonElement GenerateToi()
    {
        var source = _config.Toi;

        if (source == null)
        {
            var defaultToi = new Dictionary<string, object>
            {
                ["version"] = "1.0.0",
                ["user"] = _config.UserId,
                ["preferences"] = new Dictionary<string, object>(),
                ["privacy"] = new Dictionary<string, object>
                {
                    ["data_retention"] = "session_only",
                    ["share_with_third_parties"] = false
                }
            };

            return System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(defaultToi)).RootElement.Clone();
        }

        if (source is string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"TOI file not found: {path}");
            }

            var content = File.ReadAllText(path);
            var document = System.Text.Json.JsonDocument.Parse(content);
            return document.RootElement.Clone();
        }

        if (source is Dictionary<string, object> dict)
        {
            var merged = new Dictionary<string, object>(dict)
            {
                ["version"] = dict.GetValueOrDefault("version", "1.0.0"),
                ["user"] = dict.GetValueOrDefault("user", _config.UserId)
            };

            return System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(merged)).RootElement.Clone();
        }

        throw new ArgumentException($"Unsupported TOI source type: {source.GetType().Name}");
    }

    public async Task<FoundationResponse> ProcessInteraction(
        UserInteraction interaction,
        Channel? channel = null)
    {
        var resolvedChannel = ChannelNormalizer.Normalize(channel ?? interaction.Channel);
        var componentsInvolved = new List<string>();
        var content = new Dictionary<string, object>();

        if (_active.Rrt)
        {
            componentsInvolved.Add("rrt_advocate");
            var assessment = await Rrt.Assess(
                interaction.UserId,
                interaction.Data.GetValueOrDefault("message", "")?.ToString() ?? "",
                resolvedChannel
            );

            content["crisisAssessment"] = new Dictionary<string, object>
            {
                ["level"] = assessment.CrisisLevel.ToString(),
                ["safetyScore"] = assessment.UserSafetyScore,
                ["indicators"] = assessment.PrimaryIndicators,
                ["interventions"] = assessment.RecommendedInterventions
            };
        }

        if (_active.Swp)
        {
            componentsInvolved.Add("sleepwalker_protocol");
            var emotionalState = Sleepwalker.DetectEmotionalState(
                interaction.Data.GetValueOrDefault("message", "")?.ToString() ?? "",
                null,
                resolvedChannel,
                interaction.UserId
            );

            content["emotionalState"] = new Dictionary<string, object>
            {
                ["state"] = emotionalState.State,
                ["confidence"] = emotionalState.Confidence,
                ["indicators"] = emotionalState.Indicators
            };
        }

        if (_active.Toi)
        {
            componentsInvolved.Add("toi_otoi_framework");
            content["toi"] = _toiDocument.HasValue ? (object)_toiDocument.Value : null;
        }

        return new FoundationResponse
        {
            Timestamp = DateTime.UtcNow,
            ResponseType = "processed",
            Content = content,
            ComponentsInvolved = componentsInvolved,
            Success = true
        };
    }

    public async Task<FoundationResponse> AssessText(
        string text,
        Dictionary<string, object>? context = null,
        Channel? channel = null)
    {
        var interaction = new UserInteraction
        {
            Timestamp = DateTime.UtcNow,
            InteractionType = InteractionType.EmotionalAssessment,
            Data = new Dictionary<string, object>
            {
                ["message"] = text
            },
            UserId = _config.UserId,
            Channel = channel
        };

        return await ProcessInteraction(interaction, channel);
    }

    public async Task UpdatePreferences(Dictionary<string, object> prefs)
    {
        if (_active.Toi)
        {
            var result = ToiOtoi.ValidateTOI(prefs);
            if (!result.Valid)
            {
                var errors = result.Errors?.Select(e => new { e.Message, e.Path, e.Code }) ?? Enumerable.Empty<object>();
                throw new ArgumentException("TOI validation failed: " + System.Text.Json.JsonSerializer.Serialize(errors));
            }
        }
    }

    public Dictionary<string, object> GetSystemStatus()
    {
        return new Dictionary<string, object>
        {
            ["mode"] = _config.Mode.ToString().ToUpper(),
            ["userId"] = _config.UserId,
            ["initialized"] = _initialized,
            ["toi"] = new Dictionary<string, object>
            {
                ["generated"] = _toiDocument.HasValue,
                ["document"] = _toiDocument
            },
            ["components"] = new Dictionary<string, object>
            {
                ["toi_otoi_framework"] = _active.Toi
                    ? ToiOtoi.GetStatus()
                    : new ComponentStatus { Active = false, Mode = "disabled" },
                ["sleepwalker_protocol"] = _active.Swp
                    ? Sleepwalker.GetStatus()
                    : new ComponentStatus { Active = false, Mode = "disabled" },
                ["rrt_advocate"] = _active.Rrt
                    ? Rrt.GetStatus()
                    : new ComponentStatus { Active = false, Mode = "disabled" }
            }
        };
    }

    public async Task<HealthCheckResult> HealthCheck()
    {
        return new HealthCheckResult
        {
            Healthy = true,
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentStatus>
            {
                ["toi_otoi_framework"] = _active.Toi
                    ? new ComponentStatus { Active = true, Mode = "toi-otoi-validation" }
                    : new ComponentStatus { Active = false, Mode = "disabled" },
                ["sleepwalker_protocol"] = _active.Swp
                    ? new ComponentStatus { Active = true, Mode = "emotional-continuity" }
                    : new ComponentStatus { Active = false, Mode = "disabled" },
                ["rrt_advocate"] = _active.Rrt
                    ? new ComponentStatus { Active = true, Mode = "crisis-detection" }
                    : new ComponentStatus { Active = false, Mode = "disabled" }
            }
        };
    }

    public async Task Shutdown()
    {
        Sleepwalker.Reset();
        Rrt.Reset(_config.UserId);
        _initialized = false;
    }
}
