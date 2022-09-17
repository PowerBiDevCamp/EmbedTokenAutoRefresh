using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using EmbedTokenAutoRefresh.Models;
using Microsoft.Identity.Client;

namespace EmbedTokenAutoRefresh.Services {

  public class PowerBiServiceApi {

    private string TenantId { get; }
    private string ClientId { get; }
    private string ClientSecret { get; }

    private static string CachedToken { get; set; }
    private static DateTime CachedTokenExpires { get; set; }
    private const int MinimumTokenLifetime = 2700; // (45 minutes * 60 seconds)


    private Guid WorkspaceId { get; }
    private Guid ReportId { get; }

    private string PowerbiServiceApiRoot { get; }
    private string PowerBiServiceApiResourceId { get; }
    private string[] PowerbiDefaultScope { get; }

    public PowerBiServiceApi(IConfiguration configuration) {

      this.TenantId = configuration["AzureAd:TenantId"];
      this.ClientId = configuration["AzureAd:ClientId"];
      this.ClientSecret = configuration["AzureAd:ClientSecret"];
      this.PowerbiServiceApiRoot = configuration["PowerBi:PowerBiServiceApiRoot"];
      this.PowerBiServiceApiResourceId = configuration["PowerBi:PowerBiServiceApiResourceId"];
      this.PowerbiDefaultScope = new string[] {this.PowerBiServiceApiResourceId + "/.default"};
      this.WorkspaceId = Guid.Parse(configuration["PowerBi:workspaceId"]);
      this.ReportId = Guid.Parse(configuration["PowerBi:reportId"]);

    }

    public string GetAccessToken() {

      var app = ConfidentialClientApplicationBuilder.Create(this.ClientId)
        .WithTenantId(this.TenantId)
        .WithClientSecret(this.ClientSecret)
        .Build();

      var accessTokenRequest = app.AcquireTokenForClient(this.PowerbiDefaultScope).ExecuteAsync().Result;

      return accessTokenRequest.AccessToken;
    
    }

    public PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(PowerbiServiceApiRoot), tokenCredentials);
    }

    public async Task<ReportEmbedData> GetReportEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(WorkspaceId, ReportId);

      IList<GenerateTokenRequestV2Dataset> datasetRequests = new List<GenerateTokenRequestV2Dataset>();
      datasetRequests.Add(new GenerateTokenRequestV2Dataset(report.DatasetId));

      IList<GenerateTokenRequestV2Report> reportRequests = new List<GenerateTokenRequestV2Report>();
      reportRequests.Add(new GenerateTokenRequestV2Report(report.Id));

      GenerateTokenRequestV2 generateTokenRequest =
        new GenerateTokenRequestV2(
          datasets: datasetRequests,
          reports: reportRequests,
          lifetimeInMinutes: 10);

      // call to Power BI Service API and pass GenerateTokenRequest object to generate embed token
      var embedTokenResult = await pbiClient.EmbedToken.GenerateTokenAsync(generateTokenRequest);
     
      string formattedDate = embedTokenResult.Expiration.ToString("yyyy'-'MM'-'dd't'HH':'mm':'ss'Z'");

      // return report embedding data to caller
      return new ReportEmbedData {
        ReportId = ReportId.ToString(),
        EmbedUrl = report.EmbedUrl,
        EmbedToken = embedTokenResult.Token,
        EmbedTokenExpiration = formattedDate
      };
    }

    public async Task<EmbedTokenApiResult> GetEmbedToken() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(WorkspaceId, ReportId);

      IList<GenerateTokenRequestV2Dataset> datasetRequests = new List<GenerateTokenRequestV2Dataset>();
      datasetRequests.Add(new GenerateTokenRequestV2Dataset(report.DatasetId));

      IList<GenerateTokenRequestV2Report> reportRequests = new List<GenerateTokenRequestV2Report>();
      reportRequests.Add(new GenerateTokenRequestV2Report(report.Id));

      GenerateTokenRequestV2 generateTokenRequest = 
        new GenerateTokenRequestV2(
          datasets: datasetRequests,
          reports: reportRequests,
          lifetimeInMinutes: 10);

      
      // call to Power BI Service API and pass GenerateTokenRequest object to generate embed token
      var embedTokenResult = await pbiClient.EmbedToken.GenerateTokenAsync(generateTokenRequest);

      // return report embedding data to caller
      return new EmbedTokenApiResult {
        EmbedToken = embedTokenResult.Token,
        EmbedTokenExpiration = embedTokenResult.Expiration
      };
    }


  }
}
