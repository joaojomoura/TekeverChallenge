using Microsoft.AspNetCore.Authentication;

namespace TvShowAPI.Auth; 

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions{
    public string ApiKey { get; set; } = "SecretKey";
}