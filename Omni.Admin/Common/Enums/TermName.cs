namespace Omni.Admin.Common.Enums;

/// <summary>The words a business type renames — a copy of the main app's TermName.</summary>
public static class TermName
{
    public const string Contact = "contact";
    public const string Member = "member";
    public const string Service = "service";
    public const string Booking = "booking";
    public const string Order = "order";
    public const string Client = "client";

    public static readonly IReadOnlyList<string> All = [Contact, Member, Service, Booking, Order, Client];
}
