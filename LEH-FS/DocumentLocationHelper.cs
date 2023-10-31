using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LEH_FS
{
    public static class DocumentLocationHelper
    {
        public static void CreateCustomDocumentLocation(IOrganizationService service, Entity entity, string folderName, Guid defaultLocation)
        {
        
            QueryExpression query = new QueryExpression("sharepointdocumentlocation")
            {
                ColumnSet = new ColumnSet("sharepointdocumentlocationid")
            };
            query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, entity.Id);

            EntityCollection results = service.RetrieveMultiple(query);
        
            Entity documentLocation = new Entity("sharepointdocumentlocation");

            if (results.Entities.Any())
            {
                documentLocation = results[0];
            }
        
        

            documentLocation["name"] = folderName;
            documentLocation["regardingobjectid"] = new EntityReference(entity.LogicalName, entity.Id);

            documentLocation["parentsiteorlocation"] = new EntityReference("sharepointdocumentlocation", defaultLocation);
            documentLocation["relativeurl"] = folderName;

            service.Create(documentLocation);
        }
    }
}