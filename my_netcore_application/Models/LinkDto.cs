namespace my_netcore_application.Models
{
    public class LinkDto
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }

        public LinkDto(string href, string relation, string method)
        {
            Href = href;
            Rel = relation;
            Method = method;
        }
    }
}