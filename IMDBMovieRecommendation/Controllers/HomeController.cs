using IMDBMovieRecommendation.DTO.MovieDTOs;
using IMDBMovieRecommendation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
using IMDBMovieRecommendation.Repositories.Abstract;

namespace IMDBMovieRecommendation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMovieRepository _movieRepository;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _apiHost;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration,
            IMovieRepository movieRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiKey = _configuration.GetValue<string>("environmentVariables:API_KEY");
            _baseUrl = _configuration.GetValue<string>("environmentVariables:BASE_URL");
            _apiHost = _configuration.GetValue<string>("environmentVariables:API_HOST");
            _movieRepository = movieRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Refresh()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _apiKey);
            client.DefaultRequestHeaders.Add("x-rapidapi-host", _apiHost);
            var response = await client.GetAsync(_baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultMovieDTO>>(jsonData);
                var movies = await _movieRepository.GetMovies();
                int count = 0;
                foreach (var value in values)
                {
                    var matchingMovie = movies.FirstOrDefault(movie => movie.Title == value.Title);
                    if (matchingMovie == null)
                    {
                        await _movieRepository.CreateMovie(new CreateMovieDTO()
                        {
                            Title = value.Title,
                            Description = value.Description,
                            Year = value.Year,
                            Image = value.Image,
                            Rating = value.Rating,
                        });
                        count++;
                    }
                }
                if (count > 0)
                {
                    ViewBag.Message = $"Tebrikler. Veritabanınıza {count} yeni film eklendi. Acilen izleyin";
                }
                else
                {
                    ViewBag.Message = $"Eklenecek yeni bir film bulamadık. Listeniz hala güncel.";
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var values = await _movieRepository.GetMovies();
            return View(values);
        }
    }
}