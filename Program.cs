using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;
using System.Reflection;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TestApplicationtoCreateProject
{
    class Program
    {
        public static string personalAccessToken = "7awtul6zkle7untncpxvqq25yvwgz6fb2qr6stxozzm6qr7swxca";

        public static string orgName = "adfd365";

        public static string ImplementationmodelWItype = "Implementation Model";

        public static string ImplementationPhaseWItype = "Implementation Phase";

        public static string ImplementationstageWIType = "Implementation Stage";

        public static string PBIWIType = "Product Backlog Item";

        public static string TaskWIType = "Task";


        static async Task Main(string[] args)
        {

            var PhasesandStagesdata = await GetPhasesandstagesData();
            var ActivitiesData = await GetActivitiesData();
            var TasksData = await GetTasksData();

            var alldata = (from phandstg in PhasesandStagesdata.phasesandstages.AsEnumerable().ToList()

                           join activity in ActivitiesData.activities.AsEnumerable().ToList() on phandstg.PhaseandStageId equals activity.StageId
                           select new
                           {
                               PhaseandStageID = phandstg.PhaseandStageId,
                               AgilePhaseName = phandstg.AgilePhase,
                               TraditionalPhaseName = phandstg.TraditionalPhase,
                               StageName = phandstg.StageTitle,
                               ActivityorPBIID = activity.ActivityId,
                               ActivityorPBITitleorname = activity.PBIorActivtityTitle
                           }
                          into PhasesStagesandActivityData
                           join tasks in TasksData.Tasks.AsEnumerable().ToList() on PhasesStagesandActivityData.ActivityorPBIID equals tasks.PBIID
                           select new
                           {
                               PhaseandStageID = PhasesStagesandActivityData.PhaseandStageID,
                               AgilePhaseName = PhasesStagesandActivityData.AgilePhaseName,
                               TraditionalPhaseName = PhasesStagesandActivityData.TraditionalPhaseName,
                               StageName = PhasesStagesandActivityData.StageName,
                               ActivityorPBIID = PhasesStagesandActivityData.ActivityorPBIID,
                               ActivityorPBITitleorname = PhasesStagesandActivityData.ActivityorPBITitleorname,
                               TaskID = tasks.TaskId,
                               TaskTitleorName = tasks.TaskTitle,
                               TaskRemoteWorkData = tasks.TaskRemoteWork
                           }

                           ).ToList()
            ;



            string jsonforallstagesandactivitydata = System.Text.Json.JsonSerializer.Serialize(alldata);



            DataTable dt1 = LINQResultToDataTable(alldata);

            DataTable dt2 = LINQResultToDataTable(PhasesandStagesdata.phasesandstages.ToList());
            var projectname = "The Hershey Corporation various affiliates H Choc World";

            var phase = "Agile";

            var implementationflag = false;


            //var implmentationmodelworkItemid = Convert.ToInt32(await CreateWIinDevops("Implementation Model", projectname, ImplementationmodelWItype));

            // var phaseworkItemid = Convert.ToInt32(await createAgileandTraditionalPhasesWI(false, projectname));

            // await LinkWIinDevOpstoEachother(implmentationmodelworkItemid, phaseworkItemid);

            //var agilephasedistinct = PhasesandStagesdata.phasesandstages.AsEnumerable().ToList().Select(s=>s.AgilePhase).Distinct().ToList();
            //var traditiaonlphasedistinct = PhasesandStagesdata.phasesandstages.AsEnumerable().ToList().Select(s => s.TraditionalPhase).Distinct().ToList();

            var distincttraditionalandagilephasesresult = PhasesandStagesdata.phasesandstages.AsEnumerable().ToList().Select(s => s.AgilePhase).Distinct().ToList()
                                                          .Union(PhasesandStagesdata.phasesandstages.AsEnumerable().ToList()
                                                          .Select(s => s.TraditionalPhase).Distinct().ToList())
                                                          .Distinct().ToList();


            var stagesdatawhicharesame = alldata.Where(s => s.AgilePhaseName == s.TraditionalPhaseName).
                Select(y => new { y.StageName, y.AgilePhaseName}).Distinct().ToList();

            var stagesdatawhicharedifferentforagile = alldata.Where(s => s.AgilePhaseName != s.TraditionalPhaseName 
            && s.AgilePhaseName != "Non Applicable")
                .Select(y => new { y.StageName, y.AgilePhaseName,y.TraditionalPhaseName}).Distinct().ToList();

            var stagesdatawhicharedifferentfortraditional = alldata.Where(s => s.AgilePhaseName != s.TraditionalPhaseName 
            && s.TraditionalPhaseName != "Non Applicable").Select(y => new { y.StageName, y.TraditionalPhaseName }).Distinct().ToList();

            var phasesandstagedata = alldata.Where(s => s.AgilePhaseName != s.TraditionalPhaseName
           && s.TraditionalPhaseName != "Non Applicable" && s.AgilePhaseName != "Non Applicable")
                .Select(y => new { y.StageName, y.TraditionalPhaseName,y.AgilePhaseName }).Distinct().ToList();

            var gg = (from ag in PhasesandStagesdata.phasesandstages.AsEnumerable().ToList()

                      where ag.AgilePhase == "Non Applicable"
                     select new
                     {
                         TraditionalPhaseName = ag.TraditionalPhase,
                         
                         StageName = ag.StageTitle,


                     }).ToList();

            //var stageworkItemid = Convert.ToInt32(await CreateStageWI(true, projectname));

                     //await LinkWIinDevOpstoEachother(436, stageworkItemid);
        }

        public static async Task<string> createIMplementationModelWI(string wiTitle,string projectname,string workitemtype)
        {          
            
            
           return await CreateWIinDevops(wiTitle, projectname, workitemtype);                
            
            return workItemid;
        }

        public static async Task<string> createAgileandTraditionalPhasesWI(bool flag, string projectname)
        {
            if (flag)
            {
                workItemid = await CreateWIinDevops("Solution Planning (Presales)", projectname, "Implementation Phase");
            }
            else
            {
                workItemid = "";
            }
            return workItemid;
        }

        public static async Task<string> CreateStageWI(bool flag, string projectname)
        {
            if (flag)
            {
                workItemid = await CreateWIinDevops("Initialize Solution Planning", projectname, "Implementation Stage");
            }
            else
            {
                workItemid = "";
            }
            return workItemid;
        }

        public static async Task<string> UpdateCreatedWItoRemoved(bool flag)
        {
            if (flag)
            {
                workItemid = await CreateWIinDevops("", "", "");
            }
            else
            {
                workItemid = "";
            }
            return workItemid;
        }

        public static DataTable LINQResultToDataTable<T>(IEnumerable<T> Linqlist)
        {
            DataTable dt = new DataTable();
            PropertyInfo[] columns = null;

            if (Linqlist == null) return dt;

            foreach (T Record in Linqlist)
            {

                if (columns == null)
                {
                    columns = ((Type)Record.GetType()).GetProperties();
                    foreach (PropertyInfo GetProperty in columns)
                    {
                        Type colType = GetProperty.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dt.Columns.Add(new DataColumn(GetProperty.Name, colType));
                    }
                }

                DataRow dr = dt.NewRow();

                foreach (PropertyInfo pinfo in columns)
                {
                    dr[pinfo.Name] = pinfo.GetValue(Record, null) == null ? DBNull.Value : pinfo.GetValue
                    (Record, null);
                }

                dt.Rows.Add(dr);
            }
            return dt;
        }
        public static async Task<PhasesandStages> GetPhasesandstagesData()
        {
            var client = new HttpClient();
            var token = await GetSharepointToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //var engid = 1;
            var url = $"https://avanade.sharepoint.com/sites/Dynamics365CoP/ADF/_api/web/lists/getbytitle('Phases and Stages')/items";
            var response = await client.GetAsync(url);
            //?$filter= Released eq ‘yes'
            var listItems = await response.Content.ReadAsStringAsync();

            var jdata = JsonConvert.DeserializeObject<PhasesandStages>(listItems);

            return jdata;
        }

        public static async Task<Activites> GetActivitiesData()
        {
            var client = new HttpClient();
            var token = await GetSharepointToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //var engid = 1;
            var url = $"https://avanade.sharepoint.com/sites/Dynamics365CoP/ADF/_api/web/lists/getbytitle('Activities')/items";
            var response = await client.GetAsync(url);
            //?$filter= Released eq ‘yes'
            var listItems = await response.Content.ReadAsStringAsync();

            var jdata = JsonConvert.DeserializeObject<Activites>(listItems);

            return jdata;
        }

        public static async Task<Taskslist> GetTasksData()
        {
            var client = new HttpClient();
            var token = await GetSharepointToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //var engid = 1;
            var url = $"https://avanade.sharepoint.com/sites/Dynamics365CoP/ADF/_api/web/lists/getbytitle('Tasks')/items?$top=100000";
            var response = await client.GetAsync(url);
            //?$filter= Released eq ‘yes'
            var listItems = await response.Content.ReadAsStringAsync();

            var jdata = JsonConvert.DeserializeObject<Taskslist>(listItems);

            return jdata;
        }
        public static async Task<string> CreateWIinDevops(string WItitle,string projectName, string WIType)
        {
            var PAT = "7awtul6zkle7untncpxvqq25yvwgz6fb2qr6stxozzm6qr7swxca";
            
          
            var credentials = new VssBasicCredential(string.Empty, PAT);

            var operations = new JsonPatchDocument();
            AddPatch(operations, "/fields/System.Title", WItitle);
            AddPatch(operations, "/fields/System.Description", WItitle);
            var witClient =
                  new WorkItemTrackingHttpClient(new Uri("https://dev.azure.com/adfd365"), credentials);
            var result = await witClient.CreateWorkItemAsync(operations, projectName, WIType);
            return result.Id.ToString();
        }

        public static async Task LinkWIinDevOpstoEachother(int mainwiid,int childwiid)
        {
            var PAT = "7awtul6zkle7untncpxvqq25yvwgz6fb2qr6stxozzm6qr7swxca";
            var projectName = "The Hershey Corporation various affiliates H Choc World";
          //  var workItemType = "Product Backlog Item";

            var credentials = new VssBasicCredential(string.Empty, PAT);

         
            var operations = new JsonPatchDocument();
          
            AddPatchTest(operations, $"https://dev.azure.com/adfd365/_apis/wit/workItems/{childwiid}");
          //  AddPatch(operations, "/relations/-", values);
           // AddPatch(operations, "/fields/System.Description", "Implementation Model");
            var witClient =
                 new WorkItemTrackingHttpClient(new Uri("https://dev.azure.com/adfd365"), credentials);

            var resukt = await witClient.UpdateWorkItemAsync(operations, projectName, mainwiid);

            
        }

        

        public static void AddPatch(JsonPatchDocument document,
           string path, object value)
        {
            document.Add(new JsonPatchOperation
            {
                From = null,
                Operation = Operation.Add,
                Path = path,
                Value = value
            });
        }
        public static void AddPatchTest(JsonPatchDocument document,
           string RelUrl)
        {
            document.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Forward",
                    url = RelUrl,
                    attributes = new
                    {
                        comment = "Comment for the link"
                    }
                }
            });
        }
        public static bool createProject()
        {
            var req = new Root
            {
                name = "Deleted1 - EngagementCreationProject",
                description = "Deleted1 EngagementCreationProject-Testproject, Can be deleted any time",

                capabilities = new Capabilities
                {
                    versioncontrol = new Versioncontrol { sourceControlType = "Git" },
                    processTemplate = new ProcessTemplate
                    {
                        templateTypeId = "df7cee27-fef9-435d-a1e3-9b19c2271e30"
                    }
                }
            };

            var PersonalAccessToken = "5kygbio3st7xcs4q76nolmsgxhxe3nstbqmbnbhzoclggy2d4cfa";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://dev.azure.com/adfd365/_apis/projects?api-version=6.0");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            String authHeaer = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PersonalAccessToken)));
            httpWebRequest.Headers.Add("Authorization", "Basic " + authHeaer);
            string projstring = JsonConvert.SerializeObject(req);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                streamWriter.Write(projstring);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var streamReader = new StreamReader(httpResponse.GetResponseStream());

            var result = streamReader.ReadToEnd();

            return true;
        }

        public class Versioncontrol
        {
            public string sourceControlType { get; set; }
        }

        public class ProcessTemplate
        {
            public string templateTypeId { get; set; }
        }

        public class Capabilities
        {
            public Versioncontrol versioncontrol { get; set; }
            public ProcessTemplate processTemplate { get; set; }
        }

        public class Root
        {
            public string name { get; set; }
            public string description { get; set; }
            public int visibility { get; set; }
            public Capabilities capabilities { get; set; }
        }

        public class PhasesandStages
        {
            [JsonProperty("value")]
            public IList<Phaseandstatgedata> phasesandstages { get; set; }
        }
        public class Phaseandstatgedata
        {
            [JsonProperty("ID")]
            public string PhaseandStageId { get; set; }

            [JsonProperty("Phase")]
            public string AgilePhase { get; set; }

            [JsonProperty("Traditional_x0020_Phase")]
            public string TraditionalPhase { get; set; }

            [JsonProperty("Title")]
            public string StageTitle { get; set; }

        }

        public class PhasesStatgesActvitiesandTasksList
        {
            [JsonProperty("value")]
            public IList<PhasesStatgesActvitiesandTasksData> PhasesStatgesActvitiesandTasksListdata { get; set; }
        }
        public class PhasesStatgesActvitiesandTasksData
        {
            [JsonProperty("PhaseandStageID")]
            public string PhaseandStageID { get; set; }

            [JsonProperty("AgilePhaseName")]
            public string AgilePhaseName { get; set; }

            [JsonProperty("TraditionalPhaseName")]
            public string TraditionalPhaseName { get; set; }

            [JsonProperty("StageName")]
            public string StageNameorTitle { get; set; }

            [JsonProperty("ActivityorPBIID")]
            public string ActivityorPBIID { get; set; }

            [JsonProperty("ActivityorPBITitleorname")]
            public string ActivityorPBITitleorname { get; set; }

            [JsonProperty("TaskID")]
            public string TaskID { get; set; }


            [JsonProperty("TaskTitleorName")]
            public string TaskTitleorName { get; set; }


            [JsonProperty("TaskRemoteWorkData")]
            public string TaskRemoteWorkData { get; set; }
        }

        public class Activites
        {
            [JsonProperty("value")]
            public IList<Activitiesdata> activities { get; set; }
        }
        public class Activitiesdata
        {
            [JsonProperty("ID")]
            public string ActivityId { get; set; }

            [JsonProperty("StageId")]
            public string StageId { get; set; }

            [JsonProperty("Title")]
            public string PBIorActivtityTitle { get; set; }



        }

        public class Taskslist
        {
            [JsonProperty("value")]
            public IList<Tasksdata> Tasks { get; set; }
        }
        public class Tasksdata
        {
            [JsonProperty("ID")]
            public string TaskId { get; set; }

            [JsonProperty("PBIId")]
            public string PBIID { get; set; }

            [JsonProperty("Title")]
            public string TaskTitle { get; set; }

            [JsonProperty("Remote_x0020_Work")]
            public string TaskRemoteWork { get; set; }

        }
        public static async Task<string> GetSharepointToken()
        {
            var authClient = new HttpClient();
            var tenantId = "cf36141c-ddd7-45a7-b073-111f66d0b30c";
            var clientId = "d46b8f7d-29a0-48da-b2ad-99aeb77f58b6";

            var values = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = $"{clientId}@{tenantId}",
                ["client_secret"] = "UOdT7UbV+2eKAcOu376QdHb7g2DcevM0jNlskv3dC8k=",
                ["resource"] = $"00000003-0000-0ff1-ce00-000000000000/avanade.sharepoint.com@{tenantId}"
            };
            var tokenUrl = $"https://accounts.accesscontrol.windows.net/{tenantId}/tokens/OAuth/2";
            var response = await authClient.PostAsync(tokenUrl, new FormUrlEncodedContent(values));
            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
            return tokenResponse.AccessToken;
        }
        public static string workItemid { get; set; }
        public class TokenResponse
        {

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("ext_expires_in")]
            public int ExtExpiresIn { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("id_token")]
            public string IdToken { get; set; }

            [JsonProperty("client_info")]
            public string ClientInfo { get; set; }
        }

    }
}
