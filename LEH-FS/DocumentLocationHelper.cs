using System;
using System.Text;
using Microsoft.Xrm.Sdk;

public static class DocumentLocationHelper
{
    public static void CreateCustomDocumentLocation(IOrganizationService service, Entity entity, string folderName, string domain, string site, string library)
    {
        Entity documentLocation = new Entity("sharepointdocumentlocation");

        // Setzen Sie die notwendigen Attribute für die DocumentLocation
        documentLocation["name"] = folderName;
        documentLocation["regardingobjectid"] = new EntityReference(entity.LogicalName, entity.Id);
        
        // Setzen Sie die URL entsprechend Ihrer Anforderungen
        // string domain = "lehsolution.sharepoint.com";
        // string site = "D365Dev";
        // string library = "D365";
        // string folderName = entity.LogicalName; // Der Ordnername ist der Entity-Name ohne GUID

        documentLocation["absoluteurl"] = $"https://{domain}/{site}/{library}/{folderName}/";
        documentLocation["relativeurl"] = $"{site}/{library}/{folderName}/";

        service.Create(documentLocation);
    }
}