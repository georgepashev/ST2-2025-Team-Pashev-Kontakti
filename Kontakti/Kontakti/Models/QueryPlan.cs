namespace Kontakti.Models;
public class QueryPlan
{
    public List<Filter> Filters { get; set; } = new();
    public List<OrderBy>? Order_By { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public class Filter
    {
        public string Column { get; set; } = "";
        public string Op { get; set; } = "";
        public string? Value { get; set; }
    }
    public class OrderBy
    {
        public string Column { get; set; } = "";
        public string? Direction { get; set; } = "asc";
    }
}