using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using CsvHelper;

namespace ClutchCo
{
	public class Manager
	{
		public string GetHtmlString(string path)
		{
			var fullUrl = path;
			using var webClient = new WebClient();
			var x = webClient.DownloadString(fullUrl);
			return x;
		}

		public List<ClutchGraph> GetListClutchGraphs(string htmlCompany)
		{
			var regex1 = new Regex(@"clutchGraph"":(.*])");

			var matches = regex1.Matches(htmlCompany);
			var res = matches[0].Groups[1].Value;

			var regex = new Regex(@"\\u([0-9a-z]{4})", RegexOptions.IgnoreCase);
			var replaced = regex.Replace(res, match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));

			return JsonConvert.DeserializeObject<List<ClutchGraph>>(replaced);
		}

		public void FillNameGraphs(HtmlDocument htmlCompany, List<ClutchGraph> graphs)
		{
			foreach (var graph in graphs)
			{
				var graphName = htmlCompany.DocumentNode.SelectSingleNode($"//div[contains (@id, '{graph.ChartId}')]//..//div[contains (@class, 'h3_title')]").InnerText;
				graph.Name = graphName;
			}
		}

		public List<string> GetCompanyUrlList(string url)
		{
			var htmlDocument = GetHtmlString(url);

			var htmlSnippet = new HtmlDocument();
			htmlSnippet.LoadHtml(htmlDocument);

			var nodes = htmlSnippet.DocumentNode.SelectNodes("//h3[contains (@class, 'company-name')]//a");

			return nodes.Select(node => node.Attributes.FirstOrDefault(x => x.Name == "href")?.Value).ToList();
		}

		public Company GetCompany(string url)
		{
			try
			{
				var company = new Company();

				Console.WriteLine($"Get company: {url}");

				var htmlDocument = GetHtmlString(url);

				var htmlSnippet = new HtmlDocument();
				htmlSnippet.LoadHtml(htmlDocument);

				company.CompanyName = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//h1[contains (@class, 'page-title')]")?.InnerText);
				company.WebSite = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'quick-menu')]//li[contains (@class, 'website-link-a')]//a").Attributes.FirstOrDefault(x => x.Name == "href")?.Value);
				company.Description = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'profile-summary')]//div[contains (@property, 'description')]//p")?.InnerText);
				company.Rating = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'summary-description')]//span[contains (@class, 'rating')]")?.InnerText);
				company.MinProjectSize = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'min-project-size')]//div[contains (@class, 'field-item even')]")?.InnerText);
				company.RateRange = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'rate-range')]//div[contains (@class, 'field-item even')]")?.InnerText);
				company.SizePeople = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'size-people')]//div[contains (@class, 'field-item even')]")?.InnerText) + " people";
				company.YearFounded = DecodeHtml(htmlSnippet.DocumentNode.SelectSingleNode("//div[contains (@class, 'year-founded')]//div[contains (@class, 'field-item even')]")?.InnerText);

				var graphs = GetListClutchGraphs(htmlDocument);
				FillNameGraphs(htmlSnippet, graphs);

				company.ClutchGraphs = graphs;

				return company;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}

		}

		public void WriteToJson(List<Company> companies)
		{
			var json = JsonConvert.SerializeObject(companies);

			using var sw = new StreamWriter("companies.json", false, System.Text.Encoding.Default);

			sw.WriteLine(json);
		}

		private static string DecodeHtml(string str)
		{
			return WebUtility.HtmlDecode(str)?.Trim();
		}

		public void WriteToCsv(List<Company> companies)
		{
			Console.WriteLine("Start write to csv");

			using var writer = new StreamWriter("file1.csv");
			using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

			csv.WriteField("Company name");
			csv.WriteField("Web site");
			csv.WriteField("Rating");
			csv.WriteField("Description");
			csv.WriteField("Min project size");
			csv.WriteField("Rate range");
			csv.WriteField("Size people");
			csv.WriteField("Year founded");
			csv.WriteField("Graphs");
			csv.NextRecord();

			foreach (var company in companies)
			{
				csv.WriteField(company.CompanyName);
				csv.WriteField(company.WebSite);
				csv.WriteField(company.Rating);
				csv.WriteField(company.Description);
				csv.WriteField(company.MinProjectSize);
				csv.WriteField(company.RateRange);
				csv.WriteField(company.SizePeople);
				csv.WriteField(company.YearFounded);

				foreach (var graph in company.ClutchGraphs)
				{
					csv.WriteField(graph.ToString());
				}

				csv.NextRecord();
			}
		}
	}
}