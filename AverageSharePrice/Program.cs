using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// точка входа приложения
/// </summary>
class Program
{
    static readonly HttpClient http = new HttpClient();
    static readonly object fileLock = new object();
    const string OutputFile = "results.txt";

    static async Task<int> Main(string[] args)
    {
        // получить токен: 1. аргумент командной строки  2. env MARKETDATA_TOKEN  3. ввод с консоли
        string token = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("MARKETDATA_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Write("Введите токен MarketData (или установите MARKETDATA_TOKEN): ");
            token = Console.ReadLine()?.Trim();
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Error.WriteLine("token not provided");
            return 1;
        }

        // прочитать тикеры
        // В Visual Studio по умолчанию рабочая директория <папка_проекта>\bin\Debug\netX.Y\
        string tickersPath = "ticker.txt";

        if (!File.Exists(tickersPath))
        {
            Console.Error.WriteLine($"ticker file not found: {tickersPath}");
            return 1;
        }
        var tickers = await ReadTickersAsync(tickersPath);

        // очистить файл результата
        File.WriteAllText(OutputFile, string.Empty);

        // установить заголовок авторизации для HttpClient
        http.DefaultRequestHeaders.Clear();
        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        http.DefaultRequestHeaders.Add("Accept", "application/json");

        // ограничение параллелизма (защита от rate limit)
        int maxConcurrency = 3;
        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = new List<Task>();
        foreach (var t in tickers)
        {
            await semaphore.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await FetchAndWriteAsync(t);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{t} error: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"готово, результаты в {OutputFile}");
        return 0;
    }

    /// <summary>
    /// прочитать список тикеров по одной строке
    /// </summary>
    /// <param name="path">путь к файлу ticker.txt</param>
    /// <returns>массив тикеров</returns>
    /// <exception cref="IOException">если файл недоступен</exception>
    static async Task<string[]> ReadTickersAsync(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);
        var list = new List<string>();
        foreach (var ln in lines)
        {
            var s = ln?.Trim();
            if (!string.IsNullOrEmpty(s)) list.Add(s);
        }
        return list.ToArray();
    }

    /// <summary>
    /// получить свечи для тикера за последний год, вычислить среднюю цену и записать в файл
    /// </summary>
    /// <param name="ticker">тикер бумаги</param>
    /// <returns>задача</returns>
    /// <exception cref="HttpRequestException">если запрос неуспешен</exception>
    static async Task FetchAndWriteAsync(string ticker)
    {
        var to = DateTime.UtcNow.Date;
        var from = to.AddYears(-1);
        // API принимает формат YYYY-MM-DD
        string url = $"https://api.marketdata.app/v1/stocks/candles/D/{Uri.EscapeDataString(ticker)}/?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        await Task.Delay(1000); // 1 секунда

        using var resp = await http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"http {(int)resp.StatusCode} {resp.ReasonPhrase}");

        var json = await resp.Content.ReadAsStringAsync();
        var cand = JsonSerializer.Deserialize<CandlesResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (cand == null || cand.H == null || cand.L == null)
            throw new InvalidOperationException("invalid response");

        double avg = CalculateAverage(cand.H, cand.L);
        // сохранить результат в потокобезопасном режиме
        AppendResultThreadSafe($"{ticker}:{avg:F4}");
        Console.WriteLine($"{ticker} -> {avg:F4}");
    }

    /// <summary>
    /// вычислить среднюю цену по массивам high и low, используя (high+low)/2 для каждой строки и усредняя по дням
    /// </summary>
    /// <param name="highs">массив high</param>
    /// <param name="lows">массив low</param>
    /// <returns>средняя цена</returns>
    /// <exception cref="ArgumentException">если размер массивов не совпадает или нулевой</exception>
    static double CalculateAverage(double[] highs, double[] lows)
    {
        if (highs == null || lows == null || highs.Length != lows.Length || highs.Length == 0)
            throw new ArgumentException("invalid arrays");
        double sum = 0;
        for (int i = 0; i < highs.Length; i++) sum += (highs[i] + lows[i]) / 2.0;
        return sum / highs.Length;
    }

    /// <summary>
    /// потокобезопасно добавить строку в файл результатов (append)
    /// </summary>
    /// <param name="line">строка формата Тикер:Цена</param>
    /// <returns>void</returns>
    static void AppendResultThreadSafe(string line)
    {
        lock (fileLock)
        {
            File.AppendAllText(OutputFile, line + Environment.NewLine);
        }
    }

    /// <summary>
    /// DTO для ответа candles (ключи соответствуют API: h,l,c,o,t и т.д.)
    /// </summary>
    class CandlesResponse
    {
        [JsonPropertyName("s")] public string Status { get; set; }
        [JsonPropertyName("h")] public double[] H { get; set; }
        [JsonPropertyName("l")] public double[] L { get; set; }
        [JsonPropertyName("c")] public double[] C { get; set; }
        [JsonPropertyName("o")] public double[] O { get; set; }
        [JsonPropertyName("t")] public long[] T { get; set; }
        [JsonPropertyName("v")] public long[] V { get; set; }
    }
}
