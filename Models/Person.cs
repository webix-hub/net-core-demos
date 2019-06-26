using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebixDemos
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }

        [JsonProperty("birth_date")]
        [JsonConverter(typeof(WebixDateTimeConverter))]
        public DateTime BirthDate { get; set; }
        public bool Active { get; set; }
    }
}