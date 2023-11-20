using System;
using Microsoft.Xrm.Sdk;

namespace LEH.FieldService.Plugin.Job
{
    public class Dispatcher : IPlugin
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
                
                if (entity.Attributes.Contains("ownerid") && entity.Attributes["ownerid"] is EntityReference)
                {
                    // Holen Sie sich die EntityReference für ownerid.
                    EntityReference ownerRef = (EntityReference)entity.Attributes["ownerid"];

                    // Überprüfen Sie, ob der ownerid auf einen systemuser verweist.
                    if (ownerRef.LogicalName == "systemuser")
                    {
                        // Geben Sie die GUID des systemuser zurück.
                        // return ownerRef.Id;
                        entity["lehfs_dispouser"] =
                            new EntityReference("systemuser" , ownerRef.Id);
                        _service.Update(entity);
                    }
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