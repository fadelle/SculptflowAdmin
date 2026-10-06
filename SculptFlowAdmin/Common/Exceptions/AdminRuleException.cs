namespace SculptFlowAdmin.Common.Exceptions;

/// <summary>Thrown by services for a request an admin can fix (shown as a message on the page / a 400 from the API).</summary>
public class AdminRuleException : Exception
{
    public AdminRuleException(string message) : base(message) { }
}
