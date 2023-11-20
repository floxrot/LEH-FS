using System;
using Microsoft.Xrm.Sdk;

namespace LEH.FieldService.Plugin.Job
{
    public class Documentaion : IPlugin
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

                if (entity.GetAttributeValue<OptionSetValue>("lehfs_category") == null || entity.GetAttributeValue<OptionSetValue>("lehfs_category").Value < 546200000)
                {
                    return;
                }

                int count = 10;

                switch (entity.GetAttributeValue<OptionSetValue>("lehfs_category").Value)
                {
                    case 546200000:
                        //REP
                        count = 10;
                        break;
                    case 546200001:
                        //REP
                        count = 20;
                        break;
                }

                for (int i = 1; i <= count; i++)
                {
                    Entity ent = new Entity("lehfs_documentation");
                    ent["lehfs_documentationnumber"] = i.ToString("D3");
                    ent["lehfs_job"] = new EntityReference("lehfs_job", entity.Id);
                    _service.Create(ent);
                }
                
                
            }
            catch (Exception ex)
            {
                string className = ex.TargetSite.ReflectedType?.Name;
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