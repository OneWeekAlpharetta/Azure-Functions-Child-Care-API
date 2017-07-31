using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace ChildCareFunctionApp
{
    public static class GetChildCareServicesList
    {
        [FunctionName("GetChildCareServicesList")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            var successful = true;
            string jsonResults = "";

            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();

                string[] results = new string[14];

                results[0] = "FacilityType";
                results[1] = "Latitude";
                results[2] = "Longitude";
                results[3] = "CloseToTransit";
                results[4] = "Infants";
                results[5] = "Toddlers";
                results[6] = "Preschool";
                results[7] = "PreK";
                results[8] = "SchoolAge";
                results[9] = "Accreditation";
                results[10] = "HasEveningCare";
                results[11] = "HasDropInCare";
                results[12] = "AcceptsSubsidy";
                results[13] = "OperatingDays";

                jsonResults = JsonConvert.SerializeObject(results);
            }
            catch
            {
                successful = false;
            }

            if (successful)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResults, Encoding.UTF8, "application/json")
                };
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Unable to process your request!");
            }
        }

        public class Provider
        {
            public string Provider_Number { get; set; }
            public string Location { get; set; }
            public string County { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string MailingAddress { get; set; }
            public string MailingCity { get; set; }
            public string MailingState { get; set; }
            public string MailingZip { get; set; }
            public string Admin_First_Name { get; set; }
            public string Admin_Last_Name { get; set; }
            public string Phone { get; set; }
            public string LicenseCapacity { get; set; }
            public string Operation_Months { get; set; }
            public string Operation_Days { get; set; }
            public string Hours_Open { get; set; }
            public string Hours_Close { get; set; }
            public string Infant_0_To_12mos { get; set; }
            public string Toddler_13mos_To_2yrs { get; set; }
            public string Preschool_3yrs_To_4yrs { get; set; }
            public string Pre_K_Served { get; set; }
            public string School_Age_5yrs_Plus { get; set; }
            public string Ages_Other_Than_Pre_K_Served { get; set; }
            public string Accepts_Child_Care_Subsidy { get; set; }
            public string Has_Evening_Care { get; set; }
            public string Has_Drop_In_Care { get; set; }
            public string Has_School_Age_Summer_Care { get; set; }
            public string Has_Special_Needs_Care { get; set; }
            public string Has_Transport_ToFrom_School { get; set; }
            public string Has_School_Age_Only { get; set; }
            public string Has_Transport_ToFrom_Home { get; set; }
            public string Has_Cacfp { get; set; }
            public string Accreditation_Status { get; set; }
            public string Program_Type { get; set; }
            public string Provider_Type { get; set; }
            public string Available_PreK_Slots { get; set; }
            public string Funded_PreK_Slots { get; set; }
            public string QR_Participant { get; set; }
            public string QR_Rated { get; set; }
            public string QR_Rating { get; set; }
            public string JAN { get; set; }
            public string FEB { get; set; }
            public string MAR { get; set; }
            public string APR { get; set; }
            public string MAY { get; set; }
            public string JUN { get; set; }
            public string JULY { get; set; }
            public string AUG { get; set; }
            public string SEP { get; set; }
            public string OCT { get; set; }
            public string NOV { get; set; }
            public string DEC { get; set; }
            public string SUN { get; set; }
            public string MON { get; set; }
            public string TUE { get; set; }
            public string WED { get; set; }
            public string THU { get; set; }
            public string FRI { get; set; }
            public string SAT { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string CloseToMarta { get; set; }
        }
    }
}