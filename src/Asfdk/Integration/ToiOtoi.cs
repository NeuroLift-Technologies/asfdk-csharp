namespace Asfdk;

public static class ToiOtoi
{
    public static TOIValidationResult ValidateTOI(object candidate)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(candidate);
            var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return new TOIValidationResult
                {
                    Valid = false,
                    Errors = new List<ValidationIssue>
                    {
                        new() { Message = "TOI must be an object", Path = "", Code = "invalid_type" }
                    }
                };
            }

            var issues = new List<ValidationIssue>();

            if (!root.TryGetProperty("version", out var version) || version.ValueKind != System.Text.Json.JsonValueKind.String)
            {
                issues.Add(new ValidationIssue
                {
                    Message = "TOI must have a 'version' string field",
                    Path = "version",
                    Code = "missing_field"
                });
            }

            if (!root.TryGetProperty("user", out var user) || user.ValueKind != System.Text.Json.JsonValueKind.String)
            {
                issues.Add(new ValidationIssue
                {
                    Message = "TOI must have a 'user' string field",
                    Path = "user",
                    Code = "missing_field"
                });
            }

            if (issues.Any())
            {
                return new TOIValidationResult { Valid = false, Errors = issues };
            }

            return new TOIValidationResult
            {
                Valid = true,
                Toi = root.Clone()
            };
        }
        catch (Exception ex)
        {
            return new TOIValidationResult
            {
                Valid = false,
                Errors = new List<ValidationIssue>
                {
                    new() { Message = ex.Message, Path = "", Code = "error" }
                }
            };
        }
    }

    public static OTOIValidationResult ValidateCharter(object candidate)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(candidate);
            var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return new OTOIValidationResult
                {
                    Valid = false,
                    Errors = new List<ValidationIssue>
                    {
                        new() { Message = "Charter must be an object", Path = "", Code = "invalid_type" }
                    }
                };
            }

            return new OTOIValidationResult
            {
                Valid = true,
                Charter = root.Clone()
            };
        }
        catch (Exception ex)
        {
            return new OTOIValidationResult
            {
                Valid = false,
                Errors = new List<ValidationIssue>
                {
                    new() { Message = ex.Message, Path = "", Code = "error" }
                }
            };
        }
    }

    public static ComponentStatus GetStatus()
    {
        return new ComponentStatus { Active = true, Mode = "toi-otoi-validation" };
    }
}
