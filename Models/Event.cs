using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebixDemos
{
    public class Event
    {
        public int Id { get; set; }
        public string Text { get; set; }

        [JsonProperty("start_date")]
        [FromForm(Name = "start_date")]
        [JsonConverter(typeof(WebixDateTimeConverter))]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        [FromForm(Name = "end_date")]
        [JsonConverter(typeof(WebixDateTimeConverter))]
        public DateTime EndDate { get; set; }
    }
}