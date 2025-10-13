namespace Snapdi.Repositories.Models
{
    public class BlogSearchParameters
    {
        public string? SearchTerm { get; set; }
        public int? AuthorId { get; set; }
        public List<string>? Keywords { get; set; }
        public List<int>? KeywordIds { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}