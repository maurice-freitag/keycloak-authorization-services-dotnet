namespace Keycloak.AuthServices.Authorization.Requirements;

using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

public class RealmAccessRequirement : IAuthorizationRequirement
{
    public IReadOnlyCollection<string> Roles { get; }

    public RealmAccessRequirement(params string[] roles) => this.Roles = roles;

    public override string ToString() =>
        $"{nameof(RealmAccessRequirement)}: Roles are one of the following values: ({string.Join("|", this.Roles)})";
}

public partial class RealmAccessRequirementHandler : AuthorizationHandler<RealmAccessRequirement>
{
    private readonly ILogger<RealmAccessRequirementHandler> logger;

    public RealmAccessRequirementHandler(ILogger<RealmAccessRequirementHandler> logger)
    {
        this.logger = logger;
    }

    [LoggerMessage(100, LogLevel.Debug,
        "[{Requirement}] Access outcome {Outcome} for user {UserName}")]
    partial void DecisionAuthorizationResult(string requirement, bool outcome, string? userName);

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RealmAccessRequirement requirement)
    {
        var success = false;
        if (context.User.Claims.TryGetRealmResource(out var resourceAccess))
        {
            success = resourceAccess.Roles.Intersect(requirement.Roles).Any();

            if (success)
            {
                context.Succeed(requirement);
            }
        }

        this.DecisionAuthorizationResult(
            requirement.ToString(), success, context.User.Identity?.Name);

        return Task.CompletedTask;
    }
}
