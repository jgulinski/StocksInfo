namespace Client.Static;

public static class APIEndpoints
{
#if DEBUG
    internal const string ServerBaseUrl = "https://localhost:7013";
#else
    internal const string ServerBaseUrl = "https://appservicename.azurewebsites.net";
#endif
    internal readonly static string s_register = $"{ServerBaseUrl}/api/user/register";
    internal readonly static string s_signIn = $"{ServerBaseUrl}/api/user/signin";
}