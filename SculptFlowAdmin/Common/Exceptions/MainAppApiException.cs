using System.Net;

namespace SculptFlowAdmin.Common.Exceptions;

/// <summary>A call to the main app's platform-admin API failed in a way an admin can read (shown as the page's error message).</summary>
public class MainAppApiException : AdminRuleException
{
    public MainAppApiException(string message, HttpStatusCode? status = null) : base(message) => Status = status;
    public HttpStatusCode? Status { get; }
}
