using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//set App level const variable
private const string URL = "https://api.applicationinsights.io/v1/apps/{0}/{1}?{2}";
private const string APPID = "98afd2e0-55c5-4161-a906-d520a9fd5884";
private const string APIKEY = "dm8ox3bx5tvpbh4zhgjstc1lmbzetus2livrih6s";
private const string QUERYTYPE = "query";
//set all logs table
private const string traces  = "(traces | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string requests  = "(requests | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string pageViews  = "(pageViews | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string dependencies  = "(dependencies | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string customEvents  = "(customEvents | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string availabilityResults  = "(availabilityResults | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string exceptions  = "(exceptions | where timestamp >= datetime({0}) and timestamp < datetime({1}))";
private const string extends = " | extend itemType = iif(itemType == 'availabilityResult',itemType,iif(itemType == 'customEvent',itemType,iif(itemType == 'dependency',itemType,iif(itemType == 'pageView',itemType,iif(itemType == 'request',itemType,iif(itemType == 'trace',itemType,iif(itemType == 'exception',itemType,'')))))))";
// private const string wheres = "| where ((itemType == 'trace' or (itemType == 'request' or (itemType == 'pageView' or (itemType == 'customEvent' or (itemType == 'exception' or (itemType == 'dependency' or itemType == 'availabilityResult')))))) and (timestamp > ago({0}))";
// set initial start time and last time.
static string startTime = "2018-10-21T15:08:59.999Z";
static string endTime = "2018-10-23T02:05:59.999Z";

public static void Run(TimerInfo myTimer, ILogger log)
{
    //when the function is started, the last time is recorded at the before cycle is set as start time in current cycle
    startTime = endTime;
    //the last time is set as current time.
    endTime = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss");
    log.LogInformation("Start Time -----> " + startTime);
    log.LogInformation("End Time -----> " + endTime);
    //get logs data from last time recorded to current time.
    string data = getData(startTime, endTime);
    //parse data as line by line
    dynamic dynObj = JsonConvert.DeserializeObject<JObject>(data);
    foreach (var table in dynObj.tables)  {
        foreach (var row in table.rows) {
            dynamic item = new JObject();
            // item["table_name"] = table.name;
            // item["timestamp"] = row[0];
            // item["message"] = row[1];
            // item["severityLevel"] = row[2];
            // item["itemType"] = row[3];
            // item["customDimensions"] = row[4];
            // item["customMeasurements"] = row[5];
            // item["operation_Name"] = row[6];
            // item["operation_Id"] = row[7];
            // item["operation_ParentId"] = row[8];
            // item["operation_SyntheticSource"] = row[9];
            // item["session_Id"] = row[10];
            // item["user_Id"] = row[11];
            // item["user_AuthenticatedId"] = row[12];
            // item["user_AccountId"] = row[13];
            // item["application_Version"] = row[14];
            // item["client_Type"] = row[15];
            // item["client_Model"] = row[16];
            // item["client_OS"] = row[17];
            // item["client_IP"] = row[18];
            // item["client_City"] = row[19];
            // item["client_StateOrProvince"] = row[20];
            // item["client_CountryOrRegion"] = row[21];
            // item["client_Browser"] = row[22];
            // item["cloud_RoleName"] = row[23];
            // item["cloud_RoleInstance"] = row[24];
            // item["appId"] = row[25];
            // item["appName"] = row[26];
            // item["iKey"] = row[27];
            // item["sdkVersion"] = row[28];
            // item["itemId"] = row[29];
            // item["itemCount"] = row[30];
            // item["id"] = row[31];
            // item["source"] = row[32];
            // item["name"] = row[33];
            // item["url"] = row[34];
            // item["success"] = row[35];
            // item["resultCode"] = row[36];
            // item["duration"] = row[37];
            // item["performanceBucket"] = row[38];
            // item["target"] = row[39];
            // item["type"] = row[40];
            // item["data"] = row[41];
            // item["location"] = row[41];
            // item["size"] = row[43];
            // item["problemId"] = row[44];
            // item["handledAt"] = row[45];
            // item["assembly"] = row[46];
            // item["method"] = row[47];
            // item["outerType"] = row[48];
            // item["outerMessage"] = row[49];
            // item["outerAssembly"] = row[50];
            // item["outerMethod"] = row[51];
            // item["innermostType"] = row[52];
            // item["innermostMessage"] = row[53];
            // item["innermostAssembly"] = row[54];
            // item["innermostMethod"] = row[55];
            // item["details"] = row[56];
            var i = 0;
            foreach(var column in table.columns) {
                string property = column.name.ToString();
                item[property] = row[i];
                i ++ ;
            }
            log.LogInformation($"Log getting: {item}");
            // each line is sent to endpoint of post function as payload
            post(item.ToString(Formatting.None));
        }
    }

}

public static string getData(string start, string end) {
    // set time interval of all logs table from last time to current time
    var trace = string.Format(traces, start, end);
    var request = string.Format(requests, start, end);
    var pageView = string.Format(pageViews, start, end);
    var dependency = string.Format(dependencies, start, end);
    var customEvent = string.Format(customEvents, start, end);
    var availabilityResult = string.Format(availabilityResults, start, end);
    var exception = string.Format(exceptions, start, end);
    //make query to get all logs
    var query = "query=union " + trace + ", " + request + ", " + pageView + ", " + dependency + ", " + customEvent + ", " + availabilityResult + ", " + exception + extends;
    //make URL to get all logs
    var req = string.Format(URL, APPID, QUERYTYPE, query);
    // Create a request using URL that is getting all logs data. 
    WebRequest webRequest = WebRequest.Create(req);
    // Set the Method property of the request to GET.
    webRequest.Method = "GET";
    //set header for GET request 
    webRequest.ContentType = "application/json";
    webRequest.Headers.Add("x-api-key", APIKEY);
    //get response as stream type. 
    Stream streamResponse = webRequest.GetResponse().GetResponseStream();
    //read response
    StreamReader streamRead = new StreamReader(streamResponse);
    //convert from stream to string
    string responseString = streamRead.ReadToEnd();
    streamResponse.Close();
    return responseString;

}

public static void post(string data)
{
    // Create a request using a URL that can receive a post. 

   // WebRequest request = WebRequest.Create("https://hooks.zapier.com/hooks/catch/1710212/lv0l52/");
    WebRequest request = WebRequest.Create("https://www.scalyr.com/api/uploadLogs?token=0P67u1yoYqTsMF0gchc9braNxSHfWCgc0wM5ffhWZYT0-");
    //WebRequest request = WebRequest.Create("https://www.scalyr.com/api/uploadLogs?token=02xraYX3J8qI3wR0VlHpsHtzoeFR7IAhqvRDCpHURLfk-");
    // Set the Method property of the request to POST.
    request.Method = "POST";
    // Create POST data and convert it to a byte array.
    string postData = data;
    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
    // Set the ContentType property of the WebRequest.
    request.ContentType = "plain";
    // Set the ContentLength property of the WebRequest.
    request.ContentLength = byteArray.Length;
    // Get the request stream.
    Stream dataStream = request.GetRequestStream();
    // Write the data to the request stream.
    dataStream.Write(byteArray, 0, byteArray.Length);
    // Close the Stream object.
    dataStream.Close();
    // Get the response.
    WebResponse response = request.GetResponse();
    // Display the status.
    Console.WriteLine(((HttpWebResponse)response).StatusDescription);
    // Get the stream containing content returned by the server.
    dataStream = response.GetResponseStream();
    // Open the stream using a StreamReader for easy access.
    StreamReader reader = new StreamReader(dataStream);
    // Read the content.
    string responseFromServer = reader.ReadToEnd();
    // Display the content.
    Console.WriteLine(responseFromServer);
    // Clean up the streams.
    reader.Close();
    dataStream.Close();
    response.Close();
}



