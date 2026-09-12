namespace Asfdk;

public static class CreateFoundation
{
    public static async Task<NeuroLiftFoundation> Create(
        object userIdOrConfig,
        FoundationMode? mode = null)
    {
        FoundationConfig config;

        if (userIdOrConfig is FoundationConfig fc)
        {
            config = fc;
        }
        else if (userIdOrConfig is string userId)
        {
            config = new FoundationConfig
            {
                UserId = userId,
                Mode = mode ?? FoundationMode.Unified
            };
        }
        else
        {
            throw new ArgumentException($"Invalid configuration type: {userIdOrConfig.GetType().Name}");
        }

        var foundation = new NeuroLiftFoundation(config);
        await foundation.Initialize();
        return foundation;
    }
}
