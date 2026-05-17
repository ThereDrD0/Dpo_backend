namespace TaskManager.Api.Services;

public class DiagnosticsReportService
{
    private readonly IAppInfoService _appInfo;
    private readonly IRequestContextService _requestContext;
    private readonly ITransientMarkerService _transientMarker;

    public DiagnosticsReportService(
        IAppInfoService appInfo,
        IRequestContextService requestContext,
        ITransientMarkerService transientMarker)
    {
        _appInfo = appInfo;
        _requestContext = requestContext;
        _transientMarker = transientMarker;
    }

    public string BuildReport(string section)
    {
        return $"section={section}; app={_appInfo.AppInstanceId}; request={_requestContext.RequestId}; transient={_transientMarker.MarkerId}";
    }
}
