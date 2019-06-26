using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class WebixDateTimeConverter : DateTimeConverterBase
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return DateTime.ParseExact(reader.Value.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue( ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss") );
    }
}