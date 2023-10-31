using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LEH_FS
{
    public class Folder
    {
        public string Name { get; set; }
        public List<Folder> Subfolders { get; set; }

        private Folder()
        {
            Subfolders = new List<Folder>();
        }

        public Folder(string name) : this()
        {
            Name = name;
        }

        public void AddSubfolder(Folder folder)
        {
            Subfolders.Add(folder);
        }
    }
    
    public static class FolderParser
    {
        public static Folder ParseFromXml(string rootFolderName, string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                return new Folder(rootFolderName);
            }

            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                Folder rootFolder = new Folder(rootFolderName);
                if (doc.Root != null)
                    foreach (var subElement in doc.Root.Elements("Folder"))
                    {
                        rootFolder.AddSubfolder(ParseElement(subElement));
                    }

                return rootFolder;
            }
            catch (Exception)
            {
                // Bei einem Fehler im XML-Format wird ein leeres Root-Folder-Objekt zurückgegeben
                return new Folder(rootFolderName);
            }
        }

        private static Folder ParseElement(XElement element)
        {
            string folderName = element.Attribute("Name")?.Value;
            Folder folder = new Folder(folderName);
            foreach (var subElement in element.Elements("Folder"))
            {
                folder.AddSubfolder(ParseElement(subElement));
            }
            return folder;
        }
    }
        
        // Beispiel zur Verwendung:
        // string rootFolderName = "YourRootFolderName";
        // string xmlInput = "..."; // Ihr XML-String ohne den Root-Ordner
        // var rootFolder = FolderParser.ParseFromXml(rootFolderName, xmlInput);
        /*
        <Folder Name="Folder 1">
            <Folder Name="Subfolder 1 1">
                <Folder Name="Sub Subfolder 1 1 1" />
        ...
            </Folder>
    ...
        </Folder>
        */
}
