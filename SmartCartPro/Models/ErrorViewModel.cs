namespace SmartCartPro.Models;

public class ErrorViewModel
{
    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = "Something went wrong";
    public string UserMessage { get; set; } = "We hit a snag. Please try again in a moment.";
    /// <summary>Technical detail for browser console only — never rendered visibly on the page.</summary>
    public string? DeveloperMessage { get; set; }
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool LogDeveloperToConsole => !string.IsNullOrWhiteSpace(DeveloperMessage);
    public bool IsNotFound => StatusCode == 404;
}
