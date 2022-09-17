namespace EmbedTokenAutoRefresh.Models {

  public class ReportEmbedData {
    public string ReportId;
    public string EmbedUrl;
    public string EmbedToken;
    public string EmbedTokenExpiration { get; set; }
  }

  public class EmbedTokenApiResult{
    public string EmbedToken;
    public DateTime EmbedTokenExpiration { get; set; }
  }

}