// <snippet_AddAuthentication>
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal =  async context =>
            {
                string? sessionId = context.Principal?.Claims.First(claim => claim.Type == "sid").Value;
                IDistributedCache? cache = context.HttpContext?.RequestServices.GetService<IDistributedCache>();
                var token = cache.GetString(sessionId);
                if (cache.GetString(sessionId) == null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext?.SignOutAsync();
                }
            },
            OnSignedIn = context =>
            {
                Console.WriteLine("CookiesOnSignedIn");
                var id_token = context.Properties.Items[".Token.id_token"];
                var sessionId = new JwtSecurityToken(id_token).Claims.First(claim => claim.Type == "sid").Value;
                if (sessionId != null)
                {
                    IDistributedCache? cache = context.HttpContext?.RequestServices.GetService<IDistributedCache>();
                    var options = new DistributedCacheEntryOptions();
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    cache.SetString(sessionId, id_token, options);
                }
                return Task.CompletedTask;
            },
        };
    })
    .AddOpenIdConnect(options =>
    {
        //options.Authority = "http://host.docker.internal:8080/auth/realms/MojTelekom/"; 
        options.Authority = builder.Configuration["OIDC:AUTHORITY"]??"https://accounts.google.com";
        options.ClientId = builder.Configuration["OIDC:CLIENTID"] ?? "clientID";
        options.ResponseMode = "form_post";
        options.ResponseType = "id_token";
        options.RequireHttpsMetadata = false;
        options.SaveTokens = true;
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                Console.WriteLine("OIDCOnRedirectToIdentityProviderForSignOut");
                return Task.CompletedTask;
            },
            OnRemoteSignOut = context =>
            {
                Console.WriteLine("OIDCOnRemoteSignOut");
                //zabeleži, da je SSO seja potekla
                var logout_token = context.ProtocolMessage?.Parameters.First(item => item.Key == "logout_token").Value;
                var logout_claims = new JwtSecurityToken(logout_token).Claims;
                var logout_sessionId = logout_claims.First(claim => claim.Type == "sid").Value;
                var logout_iss = logout_claims.First(claim => claim.Type == "iss").Value;
                var logout_aud = logout_claims.First(claim => claim.Type == "aud").Value;

                IDistributedCache? cache = context.HttpContext?.RequestServices.GetService<IDistributedCache>();
                var token = cache.GetString(logout_sessionId);
                if (token != null)
                {
                    var claims = new JwtSecurityToken(logout_token).Claims;
                    var iss = claims.First(claim => claim.Type == "iss").Value;
                    var aud = claims.First(claim => claim.Type == "aud").Value;
                    if (iss == logout_iss && aud == logout_aud)
                    {
                        cache?.Remove(logout_sessionId);
                    }

                }
                context.HandleResponse();
                return Task.CompletedTask;

            },
        };
    });


//builder.Services
//    .AddStackExchangeRedisCache(options =>
//    {
//        options.Configuration = "localhost:6379";
//        options.InstanceName = "InvalidatedSessions";
//    });
builder.Services
    .AddMemoryCache();



builder.Services.AddRazorPages();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
