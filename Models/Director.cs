public class Director : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public ICollection<Movie>? Movies { get; set; }
}