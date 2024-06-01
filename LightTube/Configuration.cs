﻿using System.Text.RegularExpressions;
using InnerTube;
using Newtonsoft.Json;
using Serilog;

namespace LightTube;

public static class Configuration
{
    public static Dictionary<string, string> CustomThemeDefs { get; } = [];
    public static InnerTubeAuthorization? InnerTubeAuthorization { get; private set; }
    public static bool ApiEnabled { get; private set; }
    public static bool OauthEnabled { get; private set; }
    public static bool RegistrationEnabled { get; private set; }
    public static bool ProxyEnabled { get; private set; }
    public static bool ThirdPartyProxyEnabled { get; private set; }
    public static bool IsNightly { get; private set; }
    public static int CacheSize { get; private set; }
    public static string ConnectionString { get; private set; }
    public static string Database { get; private set; }
    public static string DefaultContentLanguage { get; private set; } = "en";
    public static string DefaultContentRegion { get; private set; } = "US";
    public static string DefaultTheme { get; private set; } = "auto";
    public static string? CustomCssPath { get; private set; }
    public static string[] Messages { get; private set; }
    public static string? Alert { get; private set; }
    public static string? AlertHash { get; private set; }
    private static Random random = new();

    private static string? GetVariable(string var, string? def = null) =>
        Environment.GetEnvironmentVariable(var) ?? def;

    public static void InitConfig()
    {
        InnerTubeAuthorization = null;
        string? authType = Environment.GetEnvironmentVariable("LIGHTTUBE_AUTH_TYPE");
        if (authType == "cookie")
        {
            Log.Error("Cookie authentication has been removed in LightTube v3 as it does not work with youtubei.googleapis.com");
        }
        else if (authType == "oauth2")
        {
            InnerTubeAuthorization = InnerTubeAuthorization.RefreshTokenAuthorization(
                Environment.GetEnvironmentVariable("LIGHTTUBE_AUTH_REFRESH_TOKEN") ??
                throw new ArgumentNullException("LIGHTTUBE_AUTH_REFRESH_TOKEN",
                    "Authentication type set to 'oauth2' but the 'LIGHTTUBE_AUTH_REFRESH_TOKEN' environment variable is not set."));
        }
        else
        {
            Log.Warning("Unknown auth type: '{AuthType}'", authType);
        }

        CustomCssPath = Environment.GetEnvironmentVariable("LIGHTTUBE_CUSTOM_CSS_PATH");
        if (CustomCssPath != null)
        {
            string contents = File.ReadAllText(CustomCssPath);
            MatchCollection matches = Regex.Matches(contents, "@themedef \"(.+?)\" (\\S+)");
            foreach (Match match in matches)
            {
                CustomThemeDefs.Add(match.Groups[2].Value, match.Groups[1].Value);
            }
        }

        ApiEnabled = GetVariable("LIGHTTUBE_DISABLE_API", "false")?.ToLower() != "true";
        OauthEnabled = GetVariable("LIGHTTUBE_DISABLE_OAUTH", "false")?.ToLower() != "true";
        RegistrationEnabled = GetVariable("LIGHTTUBE_DISABLE_REGISTRATION", "false")?.ToLower() != "true";
        ProxyEnabled = GetVariable("LIGHTTUBE_DISABLE_PROXY", "false")?.ToLower() != "true";
        ThirdPartyProxyEnabled = GetVariable("LIGHTTUBE_ENABLE_THIRD_PARTY_PROXY", "false")?.ToLower() != "true" && ProxyEnabled;

        CacheSize = int.Parse(GetVariable("LIGHTTUBE_CACHE_SIZE", "50")!);
        ConnectionString = GetVariable("LIGHTTUBE_MONGODB_CONNSTR") ?? throw new ArgumentNullException(
            "LIGHTTUBE_MONGODB_CONNSTR",
            "Database connection string is not set. Please set the 'LIGHTTUBE_MONGODB_CONNSTR' to a valid MongoDB connection string.");
        Database = GetVariable("LIGHTTUBE_MONGODB_DATABASE", "lighttube")!;
        DefaultContentLanguage = GetVariable("LIGHTTUBE_DEFAULT_CONTENT_LANGUAGE", "en")!;
        DefaultContentRegion = GetVariable("LIGHTTUBE_DEFAULT_CONTENT_REGION", "US")!;
        DefaultTheme = GetVariable("LIGHTTUBE_DEFAULT_THEME", "auto")!;
        IsNightly = GetVariable("LIGHTTUBE_IS_NIGHTLY", "false")?.ToLower() == "true";

        try
        {
            Messages = JsonConvert.DeserializeObject<string[]>(GetVariable("LIGHTTUBE_MOTD",
                "[\"Search something to get started!\"]")!)!;
        }
        catch (Exception)
        {
            Messages = ["Search something to get started!"];
        }

        Alert = GetVariable("LIGHTTUBE_ALERT");
        AlertHash = Alert != null ? Utils.Md5Sum(Alert) : null;
    }

    public static string RandomMessage() => Messages[random.Next(0, Messages.Length)];
}