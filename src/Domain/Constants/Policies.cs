namespace MyAuthorizationDemo.Domain.Constants;

public abstract class Policies
{
    public const string CanDelete = nameof(CanDelete);
    public const string CanEdit = nameof(CanEdit);
    
    public static readonly IReadOnlyDictionary<string, string> Claims = new Dictionary<string, string>()
    {
        { CanDelete, "สิทธิ์สามารถลบได้" }, { CanEdit, "สิทธิ์สามารถแก้ไขได้" }
    };
    
    public static readonly IReadOnlyDictionary<string, string> ReverseClaims = Claims.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

}
