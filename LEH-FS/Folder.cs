using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LEH_FS
{
    public class Folder
    {
        public string Name { get; set; }
        public List<Folder> Subfolders { get; set; }

        public Folder()
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
    
    public class FolderParser
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
                foreach (var subElement in doc.Root.Elements())
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
            Folder folder = new Folder(element.Name.LocalName);
            foreach (var subElement in element.Elements())
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
        <Folder1>
            <Subfolder1-1>
                <SubSubfolder1-1-1 />
                <SubSubfolder1-1-2 />
                <SubSubfolder1-1-3 />
            </Subfolder1-1>
            <Subfolder1-2>
                <SubSubfolder1-2-1 />
                <SubSubfolder1-2-2 />
                <SubSubfolder1-2-3 />
            </Subfolder1-2>
            <Subfolder1-3>
                <SubSubfolder1-3-1 />
                <SubSubfolder1-3-2 />
                <SubSubfolder1-3-3 />
            </Subfolder1-3>
        </Folder1>
        */
}
