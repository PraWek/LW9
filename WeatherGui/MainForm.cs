using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherGui
{
    /// <summary>
    /// главная форма приложения для отображения текущей погоды
    /// </summary>
    public partial class MainForm : Form
    {
        private const string ApiKey = "3479f13acefcf74bfb5a51ae6784020a"; // ключ OpenWeatherMap
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, (double lat, double lon)> _cities = new();

        /// <summary>
        /// конструктор формы
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            LoadCities();
        }

        /// <summary>
        /// загружает список городов с координатами из файла city.txt
        /// </summary>
        private void LoadCities()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "city.txt");

                if (!File.Exists(path))
                {
                    MessageBox.Show($"Файл city.txt не найден по пути:\n{path}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var lines = File.ReadAllLines(path);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл city.txt пустой", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                foreach (var line in lines)
                {
                    var ln = line.Trim();
                    if (string.IsNullOrEmpty(ln) || ln.StartsWith("#"))
                        continue;

                    var parts = ln.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3 &&
                        double.TryParse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture, out double lon))
                    {
                        string city = parts[0].Trim();
                        _cities[city] = (lat, lon);
                        comboCities.Items.Add(city);
                    }
                }

                if (comboCities.Items.Count > 0)
                {
                    comboCities.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить города — проверьте формат файла city.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении city.txt: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// обработчик нажатия кнопки получения погоды
        /// </summary>
        private async void btnGetWeather_Click(object sender, EventArgs e)
        {
            if (comboCities.SelectedItem == null)
                return;

            string city = comboCities.SelectedItem.ToString() ?? "";
            if (!_cities.TryGetValue(city, out var coords))
            {
                lblResult.Text = "Не удалось получить координаты города";
                return;
            }

            lblResult.Text = "Загрузка...";
            btnGetWeather.Enabled = false;

            try
            {
                var weather = await GetWeatherAsync(coords.lat, coords.lon);
                lblResult.Text = $"{city} ({weather.Country}) — {weather.Temp:F1}°C, {weather.Description}";
            }
            catch (Exception ex)
            {
                lblResult.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                btnGetWeather.Enabled = true;
            }
        }

        /// <summary>
        /// асинхронно получает погоду по координатам
        /// </summary>
        private async Task<Weather> GetWeatherAsync(double lat, double lon)
        {
            string url =
                $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={ApiKey}&units=metric&lang=ru";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string country = root.GetProperty("sys").GetProperty("country").GetString() ?? "";
            string name = root.GetProperty("name").GetString() ?? "";
            double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            string desc = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "";

            return new Weather
            {
                Country = country,
                Name = name,
                Temp = temp,
                Description = desc
            };
        }
    }

    /// <summary>
    /// структура данных для хранения информации о погоде
    /// </summary>
    public struct Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }
    }
}
