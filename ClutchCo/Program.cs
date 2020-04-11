using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClutchCo
{
	class Program
	{
		private const string Host = "https://clutch.co";

		private const string UrlPage0 = "https://clutch.co/web-developers?sort_by=0&min_project_size=&avg_hrly_rate=&employees=&client_focus=&industry_focus=&location%5Bcountry%5D=&form_id=spm_exposed_form&form_build_id=form-RQgzZmlmLrH_PsYK5bzSXnRxbYNqX3ERD0ZO1voiztk";

		static void Main(string[] args)
		{
			var manager = new Manager();

			var companiesUrl = new List<string>();

			var companiesInPage0 = manager.GetCompanyUrlList(UrlPage0);
			companiesUrl.AddRange(companiesInPage0);


			for (var i = 1; ; i++)
			{
				var url = $"https://clutch.co/web-developers?page={i}&sort_by=0&min_project_size=&avg_hrly_rate=&employees=&client_focus=&industry_focus=&location%5Bcountry%5D=&form_id=spm_exposed_form&form_build_id=form-RQgzZmlmLrH_PsYK5bzSXnRxbYNqX3ERD0ZO1voiztk";
				Console.WriteLine($"Get url: {url}");
				try
				{
					var companiesInPage = manager.GetCompanyUrlList(url);
					if (companiesInPage.Count == 0)
					{
						break;
					}
					companiesUrl.AddRange(companiesInPage);
					Console.WriteLine($"Count: {companiesUrl.Count}");
				}
				catch
				{
					break;
				}
			}

			var listCompanies = new List<Company>();

			//var count = 1;
			//foreach (var companyUrl in companiesUrl)
			//{
			//	Console.WriteLine($"Count = {count++}");
			//	var companyUrlReplaced = Regex.Replace(companyUrl, @".*clutch\.co", "").Trim();
			//	var company = manager.GetCompany($"{host}{companyUrlReplaced}");
			//	if (company != null)
			//	{
			//		listCompanies.Add(company);
			//	}
			//}

			Parallel.ForEach(companiesUrl, x =>
			{
				var companyUrlReplaced = Regex.Replace(x, @".*clutch\.co", "").Trim();
				var company = manager.GetCompany($"{Host}{companyUrlReplaced}");
				if (company != null)
				{
					listCompanies.Add(company);
				}
			});

			manager.WriteToJson(listCompanies);
			manager.WriteToCsv(listCompanies);

			Console.WriteLine("End");
		}
	}
}
