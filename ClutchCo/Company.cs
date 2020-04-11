using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClutchCo
{
	public class Company
	{
		[JsonProperty("companyName")]
		public string CompanyName { get; set; }

		[JsonProperty("WebSite")]
		public string WebSite { get; set; }

		[JsonProperty("Rating")]
		public string Rating { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("MinProjectSize")]
		public string MinProjectSize { get; set; }

		[JsonProperty("RateRange")]
		public string RateRange { get; set; }

		[JsonProperty("SizePeople")]
		public string SizePeople { get; set; }

		[JsonProperty("YearFounded")]
		public string YearFounded { get; set; }

		[JsonProperty("ClutchGraphs")]
		public List<ClutchGraph> ClutchGraphs { get; set; }
	}
}
