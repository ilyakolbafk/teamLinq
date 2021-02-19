using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace teamLinq
{
    internal class Program
    {
        private static readonly List<WeatherEvent> WeatherEvents = new List<WeatherEvent>();

        private static void Main()
        {
            //Нужно дополнить модель WeatherEvent, создать список этого типа List<>
            //И заполнить его, читая файл с данными построчно через StreamReader
            //Ссылка на файл https://www.kaggle.com/sobhanmoosavi/us-weather-events

            //Написать Linq-запросы, используя синтаксис методов расширений
            //и продублировать его, используя синтаксис запросов
            //(возможно с вкраплениями методов расширений, ибо иногда первого может быть недостаточно)

            //0. Linq - сколько различных городов есть в датасете.
            //1. Сколько записей за каждый из годов имеется в датасете.
            //Потом будут еще запросы

            using (var sr = new StreamReader("WeatherEvents.csv"))
            {
                sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                    WeatherEvents.Add(new WeatherEvent(line.Split(',')));
            }

            Console.WriteLine();
            DifferentCities();
            RecordsForEachYear();
            NumberOfNaturalEventsIn2018();
            DifferentStates();
            Top3RainiestCitiesIn2019();
            LongestSnowfallByYear();
        }

        //0. Linq - сколько различных городов есть в датасете.
        private static void DifferentCities()
        {
            Console.Write("Number of different cities: ");
            Console.WriteLine(
                DateTime.Now.Millisecond % 2 == 0
                    ? $"{WeatherEvents.Select(e => e.City).Distinct().Count()}\n"
                    : $"{(from e in WeatherEvents select e.City).Distinct().Count()}\n");
        }

        // 1. Сколько записей за каждый из годов имеется в датасете.
        private static void RecordsForEachYear()
        {
            Console.WriteLine("How many records for each year:");
            if (DateTime.Now.Millisecond % 2 == 0)
            {
                foreach (var year in WeatherEvents
                    .GroupBy(e => e.StartTime.Year)
                    .Select(g =>
                        new {Name = g.Key, Count = g.Count()}))
                    Console.Write($"Year {year.Name}: {year.Count} records.\n");
                Console.WriteLine();
            }
            else
            {
                foreach (var year in
                    from e in WeatherEvents
                    group e by e.StartTime.Year
                    into g
                    select new {Name = g.Key, Count = g.Count()})
                    Console.WriteLine($"Year {year.Name}: {year.Count} records.");
                Console.WriteLine();
            }
        }

        // -1. Вывести количество зафиксированных природных явлений в Америке в 2018 году.
        private static void NumberOfNaturalEventsIn2018()
        {
            Console.Write("Number of recorded natural events in America in 2018: ");
            Console.WriteLine(DateTime.Now.Millisecond % 2 == 0
                ? $"{WeatherEvents.Count(e => e.StartTime.Year == 2018)}\n"
                : $"{(from e in WeatherEvents where e.StartTime.Year == 2018 select e).Count()}\n");
        }

        // 0. Вывести количество штатов, количество городов в датасете.
        private static void DifferentStates()
        {
            Console.Write("Number of different states: ");
            Console.WriteLine(
                DateTime.Now.Millisecond % 2 == 0
                    ? $"{WeatherEvents.Select(e => e.State).Distinct().Count()}\n"
                    : $"{(from e in WeatherEvents select e.State).Distinct().Count()}\n");
        }

        // 1. Вывести топ 3 самых дождливых города в 2019 году в порядке убывания количества дождей.
        private static void Top3RainiestCitiesIn2019()
        {
            Console.WriteLine("Top 3 rainiest cities in 2019:");
            if (DateTime.Now.Millisecond % 2 == 0)
            {
                var i = 1;
                foreach (var city in WeatherEvents
                    .Where(e => e.StartTime.Year == 2019 && e.Type == WeatherEventType.Rain)
                    .GroupBy(e => e.City)
                    .Select(g => new {Name = g.Key, Count = g.Count()})
                    .OrderByDescending(g => g.Count)
                    .Take(3).ToList())
                    Console.WriteLine($"{i++}. {city.Name} ({city.Count} rains)");
            }
            else
            {
                var i = 1;
                foreach (var city in (
                        from e in WeatherEvents
                        where e.StartTime.Year == 2019 && e.Type == WeatherEventType.Rain
                        group e by e.City
                        into g
                        orderby g.Count() descending
                        select new {Name = g.Key, Count = g.Count()})
                    .Take(3).ToList())
                    Console.WriteLine($"{i++}. {city.Name} ({city.Count} rains)");
            }

            Console.WriteLine();
        }

        // 2. Вывести данные самых долгих снегопадов в Америке по годам - с какого времени, по какое время, в каком городе.

        private static void LongestSnowfallByYear()
        {
            Console.WriteLine("Longest snowfall for each year: ");
            if (DateTime.Now.Millisecond % 2 == 0)
            {
                foreach (var year in WeatherEvents
                    .Where(e => e.Type == WeatherEventType.Snow)
                    .ToLookup(e => e.StartTime.Year)
                    .Select(g => new
                    {
                        Name = g.Key, Event = g
                            .OrderByDescending(e => e.EndTime - e.StartTime).First()
                    }))
                    Console.WriteLine(
                        $"Year {year.Name}: from {year.Event.StartTime} to {year.Event.EndTime} in {year.Event.City}");
            }
            else
            {
                foreach (var year in from g in (from e in WeatherEvents where e.Type == WeatherEventType.Snow select e)
                        .ToLookup(e => e.StartTime.Year)
                    select new
                    {
                        Name = g.Key, Event =
                            (from e in g
                                orderby e.EndTime - e.StartTime descending
                                select e).First()
                    })
                    Console.WriteLine(
                        $"Year {year.Name}: from {year.Event.StartTime} to {year.Event.EndTime} in {year.Event.City}");
            }
        }

        //Дополнить модель, согласно данным из файла
        private class WeatherEvent
        {
            public string EventId { get; set; }
            public WeatherEventType Type { get; set; }
            public Severity Severity { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string TimeZone { get; set; }
            public string AirportCode { get; set; }
            public double LocationLat { get; set; }
            public double LocationLng { get; set; }
            public string City { get; set; }
            public string County { get; set; }
            public string State { get; set; }
            public int ZipCode { get; set; }

            public WeatherEvent(string[] paramOfEvent)
            {
                try
                {
                    EventId = paramOfEvent[0];
                    Type = GetEventType(paramOfEvent[1]);
                    Severity = GetSeverity(paramOfEvent[2]);
                    StartTime = DateTime.ParseExact(paramOfEvent[3],
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    EndTime = DateTime.ParseExact(paramOfEvent[4],
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    TimeZone = paramOfEvent[5];
                    AirportCode = paramOfEvent[6];
                    LocationLat = double.Parse(paramOfEvent[7]);
                    LocationLng = double.Parse(paramOfEvent[8]);
                    City = paramOfEvent[9];
                    County = paramOfEvent[10];
                    State = paramOfEvent[11];
                    if (paramOfEvent[12] != "") ZipCode = int.Parse(paramOfEvent[12]);
                }
                catch (Exception ex)
                {
                    foreach (var par in paramOfEvent)
                    {
                        Console.WriteLine(par);
                    }

                    Console.WriteLine(ex.Message);
                    Environment.Exit(0);
                }
            }

            private static WeatherEventType GetEventType(string type)
            {
                return type switch
                {
                    "Snow" => WeatherEventType.Snow,
                    "Fog" => WeatherEventType.Fog,
                    "Rain" => WeatherEventType.Rain,
                    "Cold" => WeatherEventType.Cold,
                    "Storm" => WeatherEventType.Storm,
                    "Precipitation" => WeatherEventType.Precipitation,
                    "Hail" => WeatherEventType.Hail,
                    _ => WeatherEventType.Unknown
                };
            }

            private static Severity GetSeverity(string severity)
            {
                return severity switch
                {
                    "Light" => Severity.Light,
                    "Severe" => Severity.Severe,
                    "Moderate" => Severity.Moderate,
                    "Heavy" => Severity.Heavy,
                    "Other" => Severity.Other,
                    _ => Severity.Unknown
                };
            }
        }

        //Дополнить перечисления
        enum WeatherEventType
        {
            Unknown,
            Snow,
            Fog,
            Rain,
            Cold,
            Storm,
            Precipitation,
            Hail
        }

        enum Severity
        {
            Unknown,
            Light,
            Severe,
            Moderate,
            Heavy,
            Other
        }
    }
}