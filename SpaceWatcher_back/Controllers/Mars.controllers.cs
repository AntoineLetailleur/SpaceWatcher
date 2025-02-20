using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceWatcher_back.Middlewares;


namespace SpaceWatcher_back.Controllers {

    [ApiController]
    [Route("api/v1/[controller]")]
    public class MarsController(KeycloakAuthorizationService keycloakAuthService) : ControllerBase {
        private readonly KeycloakAuthorizationService _keycloakAuthService = keycloakAuthService;

        [Authorize(Policy = "Admin")]
        [HttpGet("photos")]
        public async Task<IActionResult> GetMarsPhotos() {

            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            bool isAuthorized = await _keycloakAuthService.IsAuthorized(token, "Mars Rover Photos API");

            if (!isAuthorized) {

                return Problem(
                    title: "Authenticated user is not authorized.",
                    detail: $"You must have the Admin role.",
                    statusCode: StatusCodes.Status403Forbidden,
                    instance: HttpContext.Request.Path
                );

            } else {

                var apiKey = Environment.GetEnvironmentVariable("NASA_API_KEY");
                var endpoint = $"https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/latest_photos?api_key={apiKey}";

                try {  
                    using var client = new HttpClient();
                    var response = await client.GetAsync(endpoint);
                    Debug.WriteLine(response.StatusCode);

                    if (!response.IsSuccessStatusCode) {
                        return StatusCode((int)response.StatusCode, "Erreur lors de l'appel à l'API NASA.");
                    }

                    var data = await response.Content.ReadAsStringAsync();

                    return Ok(data);
                } catch (Exception ex) {
                    return StatusCode(500, $"Erreur interne : {ex.Message}");
                }
            }
        }
    }
}

