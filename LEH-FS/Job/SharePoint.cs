using System;
using LEH.FieldService.Plugin.Util;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LEH.FieldService.Plugin.Job
{
    public class SharePoint : IPlugin
    {
        // Globale Variablen
        private IOrganizationService _service;
        private IPluginExecutionContext _context;
        private ITracingService _tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Initialisieren der globalen Variablen
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);
            _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                // Überprüfen der Tiefe, um redundante Ausführungen zu verhindern.
                if (_context.Depth > 1)
                {
                    return;
                }

                // Hier kommt Ihre benutzerdefinierte Logik.
                
                if (!(_context.InputParameters["Target"] is Entity))
                {
                    return;
                }
                
                Entity entity = (Entity) _context.InputParameters["Target"];
                
                if (entity.LogicalName != "lehfs_job")
                {
                    return;
                }

                if (_context.MessageName != "Create")
                {
                    return;                    
                }
                
                var query = new QueryExpression("lehfs_lehfskonfiguration")
                {
                    TopCount = 1
                };
                query.ColumnSet.AddColumns(
                    "lehfs_clientid",
                    "lehfs_clientsecret",
                    "lehfs_sharepointdomain",
                    "lehfs_sharepointlibrary",
                    "lehfs_sharepointsitename",
                    "lehfs_tenantid",
                    "lehfs_xmlfolder");
                                                        
                EntityCollection col = _service.RetrieveMultiple(query);
                
                if (col.Entities.Count == 0)
                {
                    throw new Exception("No Configuration");
                }
                
                if(col.Entities.Count > 1)
                {
                    throw new Exception("Too many Configurations");
                }

                Entity config = col[0];

                string name = entity.GetAttributeValue<String>("lehfs_jobnumber");

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = entity.Id.ToString();
                }

                Folder folder = FolderParser.ParseFromXml(name, "<Folder Name=\"Root\">" + config.GetAttributeValue<String>("lehfs_xmlfolder") + "</Folder>");
                
                _tracingService.Trace(config.GetAttributeValue<String>("lehfs_xmlfolder"));

                string domain = config.GetAttributeValue<String>("lehfs_sharepointdomain");
                string tenantId = config.GetAttributeValue<String>("lehfs_tenantid");
                string clientId = config.GetAttributeValue<String>("lehfs_clientid");
                string clientSecret = config.GetAttributeValue<String>("lehfs_clientsecret");
                string library = config.GetAttributeValue<String>("lehfs_sharepointlibrary");
                string site = config.GetAttributeValue<String>("lehfs_sharepointsitename");

                if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(domain) ||
                    string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(library))
                {
                    throw new Exception("Configuration not complete");
                }

                if (site == null)
                {
                    site = string.Empty;
                }

                Guid parentLocationId = GetEntitySiteGuid();

                string token = SharePointHelper.GetAccessTokenAsync(domain, tenantId, clientId, clientSecret).GetAwaiter().GetResult();
                
                // tracingService.Trace("Access Token: " + token);

                // if (!SharePoint.DoesFolderExist(token, domain, site, libary, folder).GetAwaiter().GetResult())
                // {
                SharePointHelper.CreateFolder(token, domain, site, library, folder);
                // }
                
                DocumentLocationHelper.CreateCustomDocumentLocation(_service, entity, name, parentLocationId);



            }
            catch (Exception ex)
            {
                string className = ex.TargetSite.ReflectedType?.Name;
                throw new InvalidPluginExecutionException($"Error occurred in class {className}, file {ex.TargetSite.Module.Name} at line {GetLineNumber(ex)}: {ex.Message}. StackTrace: {ex.StackTrace}", ex);
            }
        }

        private Guid GetEntitySiteGuid()
        {
            
            var query = new QueryExpression("sharepointdocumentlocation");
            query.ColumnSet.AddColumn("sharepointdocumentlocationid");
            query.Criteria.AddCondition("relativeurl", ConditionOperator.Equal, "lehfs_job");
            EntityCollection col = _service.RetrieveMultiple(query);

            if (col.Entities.Count != 1)
            {
                Guid defaultSite = GetDefaultWebsiteGuid();
                Entity defaultWebsiteEntity = new Entity("sharepointdocumentlocation")
                {
                    ["name"] = "Defaultlocation LEH Auftrag",
                    ["parentsiteorlocation"] = new EntityReference("sharepointsite", defaultSite),
                    ["relativeurl"] = "lehfs_job"
                };
                return _service.Create(defaultWebsiteEntity);
            }

            return col[0].Id;
        }

        private Guid GetDefaultWebsiteGuid()
        {
            var query = new QueryExpression("sharepointsite");
            query.ColumnSet.AddColumn("sharepointsiteid");
            query.Criteria.AddCondition("isdefault", ConditionOperator.Equal, true);
            EntityCollection col = _service.RetrieveMultiple(query);

            if (col.Entities.Count != 1)
            {
                throw new Exception("No Defaultsite definied!");
            }

            return col[0].Id;
        }

        private int GetLineNumber(Exception ex)
        {
            // Die Zeilennummer aus der StackTrace des Exceptions extrahieren.
            var lineNumber = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            return lineNumber;
        }
    }
}