using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LEH_FS
{
    public class CJobSharePoint : IPlugin
    {
        // Globale Variablen
        private IOrganizationService service;
        private IPluginExecutionContext context;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Initialisieren der globalen Variablen
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                // Überprüfen der Tiefe, um redundante Ausführungen zu verhindern.
                if (context.Depth > 1)
                {
                    return;
                }

                // Hier kommt Ihre benutzerdefinierte Logik.
                
                if (!(context.InputParameters["Target"] is Entity))
                {
                    return;
                }
                
                Entity entity = (Entity) context.InputParameters["Target"];
                
                if (entity.LogicalName != "lehfs_job")
                {
                    return;
                }

                if (context.MessageName != "Create")
                {
                    return;                    
                }
                
                var query = new QueryExpression("lehfs_lehfskonfiguration");
                query.TopCount = 1;query.ColumnSet.AddColumns(
                    "lehfs_clientid",
                    "lehfs_clientsecret",
                    "lehfs_sharepointdomain",
                    "lehfs_sharepointlibrary",
                    "lehfs_sharepointsitename",
                    "lehfs_tenantid",
                    "lehfs_xmlfolder");
                                                        
                EntityCollection col = service.RetrieveMultiple(query);
                
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

                Folder folder = FolderParser.ParseFromXml(name, config.GetAttributeValue<String>("lehfs_xmlfolder"));

                string domain = config.GetAttributeValue<String>("lehfs_sharepointdomain");
                string tenantId = config.GetAttributeValue<String>("lehfs_tenantid");
                string clientId = config.GetAttributeValue<String>("lehfs_clientid");
                string clientSecret = config.GetAttributeValue<String>("lehfs_clientsecret");
                string libary = config.GetAttributeValue<String>("lehfs_sharepointlibrary");
                string site = config.GetAttributeValue<String>("lehfs_sharepointsitename");

                if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(domain) ||
                    string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(libary))
                {
                    throw new Exception("Configuration not complete");
                }

                if (site == null)
                {
                    site = string.Empty;
                }

                string token = SharePoint.GetSharePointAccessToken(domain, tenantId, clientId, clientSecret);

                if (!SharePoint.DoesFolderExist(token, domain, site, libary, folder))
                {
                    SharePoint.CreateFolder(token, domain, site, libary, folder);
                }
                
                DocumentLocationHelper.CreateCustomDocumentLocation(service, entity, name, domain, site, libary);



            }
            catch (Exception ex)
            {
                string className = ex.TargetSite.ReflectedType.Name;
                throw new InvalidPluginExecutionException($"Error occurred in class {className}, file {ex.TargetSite.Module.Name} at line {GetLineNumber(ex)}: {ex.Message}. StackTrace: {ex.StackTrace}", ex);
            }
        }

        private int GetLineNumber(Exception ex)
        {
            // Die Zeilennummer aus der StackTrace des Exceptions extrahieren.
            var lineNumber = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            return lineNumber;
        }
    }
}