using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using SpaceWatcher_back.Middlewares;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<KeycloakAuthorizationService>();
builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/spacewatcher";
        options.Audience = "spacewatcher-api";
        options.MetadataAddress = "http://localhost:8080/realms/spacewatcher/.well-known/openid-configuration";
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "spacewatcher-api",
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/spacewatcher",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("email_verified", "true")
        .Build())
    .AddPolicy("Admin", policy =>
        policy.RequireClaim("groups", "/USER/ADMIN"));
        
builder.Services.AddCors(options => {
        options.AddDefaultPolicy(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
});

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
