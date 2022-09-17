using Microsoft.AspNetCore.Mvc;
using EmbedTokenAutoRefresh.Models;
using EmbedTokenAutoRefresh.Services;

namespace EmbedTokenAutoRefresh.Controllers {

  [Route("api/[controller]")]
  [ApiController]
  public class EmbedTokenController : ControllerBase {

    private PowerBiServiceApi powerBiServiceApi;

    public EmbedTokenController(PowerBiServiceApi powerBiServiceApi) {
      this.powerBiServiceApi = powerBiServiceApi;
    }

    public async Task<EmbedTokenApiResult> GetEmbedToken() {
      return await this.powerBiServiceApi.GetEmbedToken();
    }

  }
}