namespace IceBreakerApp.Application.DTOs;

public class BulkOperationResult<T>
{
    public List<T> SuccessItems { get; set; } = new();
    public List<BulkOperationError> Errors { get; set; } = new();
    public int TotalProcessed => SuccessItems.Count + Errors.Count;
    public bool HasErrors => Errors.Any();
}

public class BulkOperationError
{
    public int Index { get; set; }
    public string Error { get; set; } = string.Empty;
}