namespace TeamFPT.DTO
{
    public class ValidationResults
    {
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid { get; set; } = true;
    }
}
