namespace MM.Define.Patches;

public ref struct PatchOutcome
{
    public bool WasSuccess { get; set; }
    public int ModificationCount { get; set; }
    public string ErrorMessage { get; set; }

    public PatchOutcome Fail(string errorMessage)
    {
        WasSuccess = false;
        ErrorMessage = errorMessage;
        return this;
    }
}
