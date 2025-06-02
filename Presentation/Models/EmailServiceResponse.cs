namespace Presentation.Models;

public class EmailServiceResponse
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
