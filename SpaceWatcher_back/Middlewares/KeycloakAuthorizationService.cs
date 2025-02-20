using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace SpaceWatcher_back.Middlewares {
    public class KeycloakAuthorizationService(HttpClient httpClient) {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _keycloakUrl = "http://localhost:8080/realms/spacewatcher";
        private readonly string _resourceServerClientId = "spacewatcher-api";

        public async Task<bool> IsAuthorized(string token, string resource) {
            try {
                var requestBody = new Dictionary<string, string> {
                    { "grant_type", "urn:ietf:params:oauth:grant-type:uma-ticket" },
                    { "audience", _resourceServerClientId },
                    { "permission", resource }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_keycloakUrl}/protocol/openid-connect/token") {
                    Content = new FormUrlEncodedContent(requestBody)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode) {
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var accessToken = jsonResponse.GetProperty("access_token").GetString();

                if (string.IsNullOrEmpty(accessToken)) {
                    return false;
                }

                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(accessToken);

                if (jwtToken?.Claims != null) {
                    var permissionsClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "authorization");

                    if (permissionsClaim != null) {
                        try {
                            var authorizationObj = JsonSerializer.Deserialize<JsonObject>(permissionsClaim.Value);

                            if (authorizationObj?.TryGetPropertyValue("permissions", out var permissionsNode) == true) {

                                if (permissionsNode is JsonArray permissionsArray) {

                                    foreach (var permission in permissionsArray) {

                                        if (permission is JsonObject permissionObj) {

                                            if (permissionObj.TryGetPropertyValue("rsname", out var rsname) 
                                                && rsname is not null && rsname.GetValue<string>() == resource) {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        } catch {
                            return false;
                        }
                    }
                }
                return false;
            } catch {
                return false;
            }
        }
    }
}
