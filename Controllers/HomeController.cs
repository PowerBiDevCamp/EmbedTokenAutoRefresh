using EmbedTokenAutoRefresh.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EmbedTokenAutoRefresh.Services;

namespace EmbedTokenAutoRefresh.Controllers {

  public class HomeController : Controller {

    private PowerBiServiceApi powerBiServiceApi;

    public HomeController(PowerBiServiceApi powerBiServiceApi) {
      this.powerBiServiceApi = powerBiServiceApi;
    }

    public async Task<IActionResult> Index() {
      var viewModel = await this.powerBiServiceApi.GetReportEmbeddingData(); ;
      return View(viewModel);
    }


  }
}