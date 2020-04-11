using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClutchCo
{
	public partial class ClutchGraph
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("dataset")]
		public List<Dataset> Dataset { get; set; }

		[JsonProperty("chart_id")]
		public string ChartId { get; set; }

		[JsonProperty("element_id")]
		public string ElementId { get; set; }

		[JsonProperty("containerId")]
		[JsonConverter(typeof(ParseStringConverter))]
		public long ContainerId { get; set; }


		public override string ToString()
		{
			var str = $"{Name}";
			foreach (var data in Dataset)
			{
				str += Environment.NewLine;
				str += $"{data.Label} : {data.Value}%";
			}

			var replace = str.Replace("<b>", "").Replace("</b>", "");

			var output = Regex.Replace(replace, @"<i>.*<\/i>", "").Trim();

			return output;
		}
	}

	public partial class Dataset
	{
		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("value")]
		[JsonConverter(typeof(DecodingChoiceConverter))]
		public long Value { get; set; }

		[JsonProperty("type")]
		public TypeEnum Type { get; set; }
	}

	public enum TypeEnum { Main, Small };

	public partial class ClutchGraph
	{
		public static List<ClutchGraph> FromJson(string json) => JsonConvert.DeserializeObject<List<ClutchGraph>>(json, Converter.Settings);
	}

	public static class Serialize
	{
		public static string ToJson(this List<ClutchGraph> self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				TypeEnumConverter.Singleton,
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}

	internal class ParseStringConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			var value = serializer.Deserialize<string>(reader);
			long l;
			if (Int64.TryParse(value, out l))
			{
				return l;
			}
			throw new Exception("Cannot unmarshal type long");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);
				return;
			}
			var value = (long)untypedValue;
			serializer.Serialize(writer, value.ToString());
			return;
		}

		public static readonly ParseStringConverter Singleton = new ParseStringConverter();
	}

	internal class TypeEnumConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			var value = serializer.Deserialize<string>(reader);
			switch (value)
			{
				case "main":
					return TypeEnum.Main;
				case "small":
					return TypeEnum.Small;
			}
			throw new Exception("Cannot unmarshal type TypeEnum");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);
				return;
			}
			var value = (TypeEnum)untypedValue;
			switch (value)
			{
				case TypeEnum.Main:
					serializer.Serialize(writer, "main");
					return;
				case TypeEnum.Small:
					serializer.Serialize(writer, "small");
					return;
			}
			throw new Exception("Cannot marshal type TypeEnum");
		}

		public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
	}

	internal class DecodingChoiceConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			switch (reader.TokenType)
			{
				case JsonToken.Integer:
					var integerValue = serializer.Deserialize<long>(reader);
					return integerValue;
				case JsonToken.String:
				case JsonToken.Date:
					var stringValue = serializer.Deserialize<string>(reader);
					long l;
					if (Int64.TryParse(stringValue, out l))
					{
						return l;
					}
					break;
			}
			throw new Exception("Cannot unmarshal type long");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);
				return;
			}
			var value = (long)untypedValue;
			serializer.Serialize(writer, value);
			return;
		}

		public static readonly DecodingChoiceConverter Singleton = new DecodingChoiceConverter();
	}
}
