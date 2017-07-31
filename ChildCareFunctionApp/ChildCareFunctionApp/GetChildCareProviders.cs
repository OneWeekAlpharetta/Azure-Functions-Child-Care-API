using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System.Configuration;

namespace ChildCareFunctionApp
{
    public static class GetChildCareProviders
    {
        [FunctionName("GetChildCareProviders")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var successful = true;
            var results = "";
            var badReturnMessage = "";

            dynamic data = await req.Content.ReadAsAsync<object>();

            Provider p = new Provider();

            //log.Info(data.ToString());

            try
            {
                p = JsonConvert.DeserializeObject<Provider>(data.ToString());
            }
            catch (Exception e)
            {
                log.Info("error: " + e.ToString());
                successful = false;
                badReturnMessage = "Unable to deserialize request";
                return req.CreateResponse(HttpStatusCode.BadRequest, badReturnMessage);
            }

            string filter = "";
            try
            {
                filter = BuildFilterMessage(log, p);
                if (String.IsNullOrEmpty(filter))
                {
                    successful = false;
                    badReturnMessage = "Unable to make query";
                    return req.CreateResponse(HttpStatusCode.BadRequest, badReturnMessage);
                }

            }
            catch (Exception e)
            {
                log.Info("error: " + e.ToString());
                successful = false;
                badReturnMessage = "Unable to make query";
                return req.CreateResponse(HttpStatusCode.BadRequest, badReturnMessage);
            }

            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(log);
            results = RunFilterQuery(log, indexClientForQueries, filter);
            //  results = RunQueries(log, indexClientForQueries); 

            if (successful)
            {
                return req.CreateResponse(HttpStatusCode.OK, results);
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, badReturnMessage);
            }
        }

        private static string BuildFilterMessage(TraceWriter log, Provider p)
        {
            log.Info("Building Filter Message");

            string filter = "";
            bool isFirst = true;

            if (!String.IsNullOrEmpty(p.FacilityType))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "FacilityType eq '" + p.FacilityType + "'";
                isFirst = false;
            }

            //  if (p.Latitude != null)
            // {
            //     if (!isFirst)
            //     {
            //          filter += " and " ;
            //     }
            //     filter += "Latitude eq " + p.Latitude;         
            //     isFirst = false;
            // }

            // if (p.Longitude != null)
            // {
            //     if (!isFirst)
            //     {
            //          filter += " and " ;
            //     }
            //     filter += "Longitude eq " + p.Longitude;         
            //     isFirst = false;
            // }

            if (!String.IsNullOrEmpty(p.CloseToTransit))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "CloseToTransit eq " + p.CloseToTransit.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.Infants))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "Infants eq " + p.Infants.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.Toddlers))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "Toddlers eq " + p.Toddlers.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.Preschool))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "Preschool eq " + p.Preschool.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.PreK))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "PreK eq " + p.PreK.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.SchoolAge))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "SchoolAge eq " + p.SchoolAge.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.ProgramType))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "ProgramType eq '" + p.ProgramType + "'";
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.Accreditation))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "Accreditation eq '" + p.Accreditation + "'";
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.HasEveningCare))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "HasEveningCare eq " + p.HasEveningCare.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.HasDropInCare))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "HasDropInCare eq " + p.HasDropInCare.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.AcceptsSubsidy))
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "AcceptsSubsidy eq " + p.AcceptsSubsidy.ToLower();
                isFirst = false;
            }

            if (!String.IsNullOrEmpty(p.OperatingDays))
            {

                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "OperatingDays eq '" + p.OperatingDays + "'";
                isFirst = false;
            }

            if (p.Longitude != null && p.Latitude != null)
            {
                if (!isFirst)
                {
                    filter += " and ";
                }
                filter += "geo.distance(location, geography'POINT(" + p.Latitude + " " + p.Longitude + ")') le 10";

                isFirst = false;
            }

            log.Info(filter);

            return filter;
            // return "AcceptsSubsidy eq true";
        }



        private static string RunFilterQuery(TraceWriter log, ISearchIndexClient indexClient, string filter)
        {
            SearchParameters parameters;
            DocumentSearchResult<Provider> results;

            log.Info("Apply a filter to the index and return all fields:\n");

            parameters =
                new SearchParameters()
                {
                // Filter =  "City eq 'Lithonia' and id eq 'CCLC-35302'",
                OrderBy = new[] { "search.score() asc" },
                    Filter = filter,
                    Select = new[] { "*" }
                };

            try
            {
                results = indexClient.Documents.Search<Provider>("*", parameters);
            }
            catch (Exception e)
            {
                log.Info("Exception in run filter query");
                log.Info("Exception" + e.ToString());
                results = null;
                return null;
            }

            WriteDocuments(log, results);

            string json = JsonConvert.SerializeObject(results);

            return json;
        }




        private static SearchIndexClient CreateSearchIndexClient(TraceWriter log)
        {
            string searchServiceName = ConfigurationSettings.AppSettings["AzureSearchServiceName"];
            string queryApiKey = ConfigurationSettings.AppSettings["AzureSearchQueryKey"];
            string index = ConfigurationSettings.AppSettings["AzureSearchIndex"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, index, new SearchCredentials(queryApiKey));
            return indexClient;
        }

        private static string RunQueries(TraceWriter log, ISearchIndexClient indexClient)
        {
            SearchParameters parameters;
            DocumentSearchResult<Provider> results;

            log.Info("Search the entire index for the term 'Lithonia' and return all fields:\n");

            parameters =
                new SearchParameters()
                {
                    Select = new[] { "*" }
                };

            try
            {
                results = indexClient.Documents.Search<Provider>("Lithonia", parameters);
            }
            catch (Exception e)
            {
                log.Info("Exception in Run Queries");
                log.Info("Exception" + e.ToString());
                results = null;
            }

            //WriteDocuments(log, results);

            string json = JsonConvert.SerializeObject(results);

            return json;
        }




        private static void WriteDocuments(TraceWriter log, DocumentSearchResult<Provider> searchResults)
        {
            foreach (SearchResult<Provider> result in searchResults.Results)
            {
                log.Info((result.Document).ToString());
            }
        }






        public partial class Provider
        {
            [System.ComponentModel.DataAnnotations.Key]
            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string id { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string Facility { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string FacilityType { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string PhoneNumber { get; set; }

            [IsSearchable]
            public string AddressLine1 { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string City { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string State { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string ZipCode { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public double? Latitude { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public double? Longitude { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string CloseToTransit { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string Infants { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string Toddlers { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string Preschool { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string PreK { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string SchoolAge { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string ProgramType { get; set; }

            [IsSearchable, IsFilterable, IsSortable, IsFacetable]
            public string Accreditation { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string HasEveningCare { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string HasDropInCare { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string AcceptsSubsidy { get; set; }

            [IsFilterable, IsSortable, IsFacetable]
            public string OperatingDays { get; set; }

            [IsFilterable, IsSortable]
            public GeographyPoint Location { get; set; }
        }

        public partial class Provider
        {
            public override string ToString()
            {
                var builder = new StringBuilder();

                if (!String.IsNullOrEmpty(id))
                {
                    builder.AppendFormat("ID: {0}\t", id);
                }

                if (!String.IsNullOrEmpty(Facility))
                {
                    builder.AppendFormat("Facility: {0}\t", Facility);
                }

                if (!String.IsNullOrEmpty(FacilityType))
                {
                    builder.AppendFormat("Facility Type: {0}\t", FacilityType);
                }

                if (!String.IsNullOrEmpty(PhoneNumber))
                {
                    builder.AppendFormat("PhoneNumber: {0}\t", PhoneNumber);
                }

                if (!String.IsNullOrEmpty(AddressLine1))
                {
                    builder.AppendFormat("AddressLine1: {0}\t", AddressLine1);
                }

                if (!String.IsNullOrEmpty(City))
                {
                    builder.AppendFormat("City: {0}\t", City);
                }

                if (!String.IsNullOrEmpty(State))
                {
                    builder.AppendFormat("State: {0}\t", State);
                }

                if (!String.IsNullOrEmpty(ZipCode))
                {
                    builder.AppendFormat("ZipCode: {0}\t", ZipCode);
                }

                if (Latitude != null)
                {
                    builder.AppendFormat("Latitude: {0}\t", Latitude);
                }

                if (Longitude != null)
                {
                    builder.AppendFormat("Longitude: {0}\t", Longitude);
                }

                if (!String.IsNullOrEmpty(CloseToTransit))
                {
                    builder.AppendFormat("CloseToTransit included: {0}\t", CloseToTransit);
                }

                if (!String.IsNullOrEmpty(Infants))
                {
                    builder.AppendFormat("Infants included: {0}\t", Infants);
                }

                if (!String.IsNullOrEmpty(Toddlers))
                {
                    builder.AppendFormat("Toddlers included: {0}\t", Toddlers);
                }

                if (!String.IsNullOrEmpty(Preschool))
                {
                    builder.AppendFormat("Preschool included: {0}\t", Preschool);
                }

                if (!String.IsNullOrEmpty(PreK))
                {
                    builder.AppendFormat("PreK included: {0}\t", PreK);
                }

                if (!String.IsNullOrEmpty(SchoolAge))
                {
                    builder.AppendFormat("SchoolAge included: {0}\t", SchoolAge);
                }

                if (!String.IsNullOrEmpty(ProgramType))
                {
                    builder.AppendFormat("ProgramType: {0}\t", ProgramType);
                }

                if (!String.IsNullOrEmpty(Accreditation))
                {
                    builder.AppendFormat("Accreditation: {0}\t", Accreditation);
                }

                if (!String.IsNullOrEmpty(HasEveningCare))
                {
                    builder.AppendFormat("HasEveningCare: {0}\t", HasEveningCare);
                }

                if (!String.IsNullOrEmpty(HasDropInCare))
                {
                    builder.AppendFormat("HasDropInCare: {0}\t", HasDropInCare);
                }

                if (!String.IsNullOrEmpty(AcceptsSubsidy))
                {
                    builder.AppendFormat("AcceptsSubsidy: {0}\t", AcceptsSubsidy);
                }

                if (!String.IsNullOrEmpty(OperatingDays))
                {
                    builder.AppendFormat("OperatingDays: {0}\t", OperatingDays);
                }

                if (Location != null)
                {
                    builder.AppendFormat("Location: Latitude {0}, longitude {1}\t", Location.Latitude, Location.Longitude);
                }

                // if (Tags != null && Tags.Length > 0)
                // {
                //     builder.AppendFormat("Tags: [{0}]\t", String.Join(", ", Tags));
                // }

                return builder.ToString();
            }
        }
    }
}