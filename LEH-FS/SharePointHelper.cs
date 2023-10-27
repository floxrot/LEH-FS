// using System;
// using System.Collections.Generic;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Text;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
//
// namespace LEH_FS
// {
//     public class SharePointHelper
//     {
//         public static string GetAccessToken(string clientId, string clientSecret, string domain, string tenantId)
//         {
//             using (HttpClient client = new HttpClient())
//             {
//                 string siteUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
//                 var content = new FormUrlEncodedContent(new[]
//                 {
//                     new KeyValuePair<string, string>("resource", $"https://{domain}.sharepoint.com/"),
//                     new KeyValuePair<string, string>("client_id", clientId),
//                     new KeyValuePair<string, string>("client_secret", clientSecret),
//                     new KeyValuePair<string, string>("grant_type", "client_credentials")
//                 });
//
//                 var response = client.PostAsync(siteUrl, content).Result;
//                 var responseBody = response.Content.ReadAsStringAsync().Result;
//
//                 if (string.IsNullOrWhiteSpace(responseBody))
//                 {
//                     throw new Exception("The response body is empty.");
//                 }
//
//                 JObject jObject;
//                 try
//                 {
//                     jObject = JObject.Parse(responseBody);
//                 }
//                 catch (JsonReaderException ex)
//                 {
//                     throw new Exception($"Failed to parse the response body: {responseBody}", ex);
//                 }
//
//                 return jObject["access_token"].ToString();
//             }
//         }
//
//         public static void CreateFolder(string token, string domain, string site, string library, Folder folder)
//         {
//             using (HttpClient client = new HttpClient())
//             {
//                 string siteUrl = $"https://{domain}/sites/{site}/_api/web/folders";
//                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                 client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//
//                 string body = "{ '__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '/sites/" + site + "/" + library +"/" + folder.Name + "' }";
//                 var content = new StringContent(body, Encoding.UTF8, "application/json");
//                 var response = client.PostAsync(siteUrl, content).Result;
//                 if (!response.IsSuccessStatusCode)
//                 {
//                     var errorDetails = response.Content.ReadAsStringAsync().Result;
//                     throw new Exception($"Error creating SharePoint folder: {response.StatusCode}. Details: {errorDetails}");
//                 }
//                 if (folder.Subfolders.Count > 0)
//                 {
//                     foreach (Folder subFolder in folder.Subfolders)
//                     {
//                         CreateFolder(token, domain, site,
//                             library, folder);
//                     }
//                 }
//             }
//         }
//     }
// }