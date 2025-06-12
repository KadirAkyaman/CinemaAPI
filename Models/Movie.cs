public class Movie : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Genre { get; set; }
    public Director Director { get; set; }
    public int DirectorId { get; set; }
}