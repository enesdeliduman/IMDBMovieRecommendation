namespace IMDBMovieRecommendation.DTO.MovieDTOs;

public class CreateMovieDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string Rating { get; set; }
    public string Year { get; set; }
    public bool IsWatching { get; set; } = false;
    public string Comment { get; set; } = string.Empty;
}