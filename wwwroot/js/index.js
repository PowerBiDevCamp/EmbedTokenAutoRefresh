
$(function () {

  // set report container height
  let windowHeight = window.innerHeight;
  let containerTop = $("#report-container")[0].getBoundingClientRect().top;
  let buffer = 8;
  $("#report-container").height(windowHeight - containerTop - buffer);


  let models = window["powerbi-client"].models;
  let reportContainer = $("#report-container").get(0);

  let reportEmbedData = window.ReportEmbedData;

  console.log("data: ", reportEmbedData);

  let reportId = reportEmbedData.ReportId;
  let embedUrl = reportEmbedData.EmbedUrl;
  let embedToken = reportEmbedData.EmbedToken;
  let embedTokenExpiration = reportEmbedData.EmbedTokenExpiration;

  reportLoadConfig = {
    type: "report",
    id: reportId,
    embedUrl: embedUrl,
    accessToken: embedToken,
    tokenType: models.TokenType.Embed,
    settings: {
      panes: {
        filters: { expanded: false, visible: false},
        pageNavigation: { visible: true, position: models.PageNavigationPosition.Left }
      }
    }

  };

  // Embed Power BI report when Access token and Embed URL are available
  // The access token is valid for 60 minutes and will need to be refreshed after that
  let report = powerbi.embed(reportContainer, reportLoadConfig);

  let refreshEmbedToken = async () => {
    $("#expiration").text("refreshing token...");
    let data = await $.ajax({ url: "/api/EmbedToken" });
    embedToken = data.embedToken;
    embedTokenExpiration = data.embedTokenExpiration;
    report.setAccessToken(embedToken);
  };

  let checkEmbedToken = () => {

    let expiration = new Date(embedTokenExpiration);
    let now = new Date();

    let secondsToExpire = Math.floor((expiration.getTime() - now.getTime()) / 1000);

    var minutes = Math.floor(secondsToExpire / 60);
    var seconds = secondsToExpire % 60;
    var statusMessage = "Token expires: " + String(minutes).padStart(2, "0") + ":" + String(seconds).padStart(2, "0");

    console.log(statusMessage);
    $("#expiration").text(statusMessage);

    // refresh embed token five minutes before it expires
    if (secondsToExpire < ( 60 * 5 )) { 
      refreshEmbedToken();
    }

  }

  checkEmbedToken();

  // Clear any other loaded handler events
  report.off("loaded");

  // Triggers when a report schema is successfully loaded
  report.on("loaded", function () {
    console.log("Report load successful");
  });

  // Clear any other rendered handler events
  report.off("rendered");

  // Triggers when a report is successfully embedded in UI
  report.on("rendered", function () {
    console.log("Report render successful");

    setInterval(checkEmbedToken, 5000);
  });

  // Clear any other error handler events
  report.off("error");

  // Handle embed errors
  report.on("error", function (event) {
    var errorMsg = event.detail;

    // Use errorMsg variable to log error in any destination of choice
    console.error(errorMsg);
    return;
  });

  $("#fullscreen").click(() => {
    console.log("Here here");
    report.fullscreen();
  });

});