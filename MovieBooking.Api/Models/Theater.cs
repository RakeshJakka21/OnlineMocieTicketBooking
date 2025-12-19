
namespace MovieBooking.Api.Models
{
    public class Theater
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}
