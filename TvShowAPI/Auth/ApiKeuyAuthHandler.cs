using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace TvShowAPI.Auth; 

public class ApiKeuyAuthHandler : AuthenticationHandler<ApiKeyAuthSchemeOptions> {
    public ApiKeuyAuthHandler(IOptionsMonitor<ApiKeyAuthSchemeOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
        : base(options, logger, encoder, clock) {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        
        var header = Request.Headers[HeaderNames.Authorization].ToString();
        
        if(header != Options.ApiKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

        var claims = new[] {
            new Claim(ClaimTypes.Email, "joao@joaomoura.pt"),
            new Claim(ClaimTypes.Name, "joaomoura")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "ApiKey");
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(claimsIdentity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}