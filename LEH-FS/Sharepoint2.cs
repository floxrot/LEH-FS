// using System;
// using System.IO;
// using System.Net;
// using System.Text;
//
// namespace LEH_FS
// {
//     public static class Sharepoint2
//     {
//         static string GetTenantId(string sharepointSiteToPing)
//         {
//             string tennantId = string.Empty;
//             WebRequest request = WebRequest.Create(sharepointSiteToPing);
//             request.Headers.Add("Authorization: Bearer ");
//             try
//             {
//                 using (request.GetResponse())
//                 {
//                 }
//             }
//             catch (WebException e)
//             {
//                 string bearerResponseHeader = e.Response.Headers["WWW-Authenticate"];
//                 const string bearer = "Bearer realm=\"";
//                 int bearerIndex = bearerResponseHeader.IndexOf(bearer, StringComparison.Ordinal);
//                 int realmIndex = bearerIndex + bearer.Length;
//                 string resource = string.Empty;
//                 if (bearerResponseHeader.Length >= realmIndex + 36)
//                 {
//                     tennantId = bearerResponseHeader.Substring(realmIndex, 36);
//                     Guid realmGuid;
//                     if (Guid.TryParse(tennantId, out realmGuid))
//                     {
//                     }
//                 }
//
//                 const string client = "client_id=\"";
//                 int clientIndex = bearerResponseHeader.IndexOf(client, StringComparison.Ordinal);
//                 int clientIdIndex = clientIndex + client.Length;
//                 if (bearerResponseHeader.Length >= clientIdIndex + 36)
//                 {
//                     resource = bearerResponseHeader.Substring(clientIdIndex, 36);
//                     Guid resourceGuid;
//                     if (Guid.TryParse(resource, out resourceGuid))
//                     {
//                     }
//                 }
//             }
//             catch (Exception)
//             {
//                 // Something else happened.Rethrow or log.
//                 throw;
//             }
//
//             return tennantId;
//         }
//
//         static string GetAuthorisationToken(string sharepointDomain, string tennantId, string resource, string clientId,
//             string clientSecret)
//         {
//             string access_token = string.Empty;
//             WebRequest request = WebRequest.Create("https: //accounts.accesscontrol.windows.net/" + tennantId + "/tokens/OAuth/2");
//             request.Method = "POST";
//             // Create POST data and convert it to a byte array. 
//             string postData = "grant_type = client_credentials" +
//                 "&client_id = " +WebUtility.UrlEncode(clientId + "@" + tennantId) +
//                 " & client_secret = " +WebUtility.UrlEncode(clientSecret) +
//                 " & resource = " +WebUtility.UrlEncode(resource + " /" + sharepointDomain + "@" +tennantId);
//             byte[] byteArray = Encoding.UTF8.GetBytes(postData);
//             // Set the ContentType property of the WebRequest. 
//             request.ContentType = "application / x - www - form - urlencoded";
//             // Set the ContentLength property of the WebRequest. 
//             request.ContentLength = byteArray.Length;
//             // Get the request stream. 
//             Stream dataStream = request.GetRequestStream();
//             // Write the data to the request stream. 
//             dataStream.Write(byteArray, 0, byteArray.Length);
//             // Close the Stream object. 
//             dataStream.Close();
//             try
//             {
//                 using (WebResponse response = request.GetResponse())
//                 {
//                     // Display the status. 
//                     Console.WriteLine("Status of Token Request: " +((HttpWebResponse) response).StatusDescription);
//                     // Get the stream containing content returned by the server. 
//                     dataStream = response.GetResponseStream();
//                     // Open the stream using a StreamReader for easy access. 
//                     StreamReader reader = new StreamReader(dataStream);
//                     // Read the content. 
//                     string responseFromServer = reader.ReadToEnd();
//                     // Clean up the streams. 
//                     reader.Close();
//                     dataStream.Close();
//                     //Get accesss token
//                     const string accessToken = "access_token\":\"";
//                     int clientIndex = responseFromServer.IndexOf(accessToken, StringComparison.Ordinal);
//                     int accessTokenIndex = clientIndex + accessToken.Length;
//                     access_token = responseFromServer.Substring(accessTokenIndex,
//                         (responseFromServer.Length - accessTokenIndex - 2));
//                     return access_token;
//                 }
//             }
//             catch (WebException wex)
//             {
//                 HttpWebResponse httpResponse = wex.Response as HttpWebResponse;
//                 // resource was not modified.
//                 Console.WriteLine("Access token not retrieved: " +httpResponse.StatusDescription);
//                 // Something else happened. Rethrow or log.
//                 throw;
//             }
//             catch (Exception)
//             {
//                 // Something else happened. Rethrow or log.
//                 throw;
//             }
//         }
//
//         static void Createfolder(string accessToken, string sharePointSite, string library, string folder)
//         {
//             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sharePointSite + "/_api/lists/getByTitle(‘" + library + "‘)/rootfolder/folders/add(url=’" + folder + "‘)");
//             request.Method = "POST";
//             request.Accept = "application/json;odata=verbose";
//             request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
//             request.ContentLength = 0;
//             try
//             {
//                 using (WebResponse response = request.GetResponse())
//                 {
//                     // Do something if the resource has changed.
//                 }
//             }
//             catch (WebException wex)
//             {
//                 HttpWebResponse httpResponse = wex.Response as HttpWebResponse;
//                 if (httpResponse.StatusCode == HttpStatusCode.NotModified)
//                 {
//                     // resource was not modified.
//                 }
//                 // Something else happened. Rethrow or log.
//                 throw;
//             }
//             catch (Exception)
//             {
//                 // Something else happened. Rethrow or log.
//                 throw;
//             }
//         }
//     }
// }