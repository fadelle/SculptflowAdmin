using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SculptFlowAdmin.Common.Exceptions;

/// <summary>A billing API call failed in a way an admin can read (shown as the page's error message).</summary>
public class BillingApiException : AdminRuleException
{
    public BillingApiException(string message, HttpStatusCode? status = null) : base(message) => Status = status;
    public HttpStatusCode? Status { get; }
}
