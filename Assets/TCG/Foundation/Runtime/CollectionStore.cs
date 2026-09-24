using System;
using System.IO;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed class CollectionStore
    {
        public string PathName {get;}
        public bool CanWrite {get;private set;}=true;
        public string Warning {get;private set;}="";
        public CollectionStore(string directory){PathName=Path.Combine(directory,"collection-v1.json");}
        public CollectionLibrary Load(ContentCatalog catalog)
        {
            if(!File.Exists(PathName))return new CollectionLibrary(catalog,new CollectionData());
            try{return Read(catalog,PathName);}
            catch(Exception e)
            {
                CanWrite=false;Warning="Coleção não carregada: "+e.Message+". Arquivo preservado; salvamento bloqueado.";
                try{var recovered=Read(catalog,PathName+".bak");Warning+=" Exibindo a cópia de segurança.";return recovered;}catch{ return new CollectionLibrary(catalog,new CollectionData()); }
            }
        }
        CollectionLibrary Read(ContentCatalog catalog,string path)
        {
            var text=File.ReadAllText(path);if(!text.TrimStart().StartsWith("{")||!text.Contains("\"schemaVersion\"")||!text.Contains("\"owned\"")||!text.Contains("\"decks\""))throw new InvalidDataException("JSON incompleto.");
            var data=JsonUtility.FromJson<CollectionData>(text);if(data==null)throw new InvalidDataException("JSON vazio.");if(data.schemaVersion>=2&&(!text.Contains("\"coins\"")||!text.Contains("\"cosmetics\"")))throw new InvalidDataException("Economia incompleta.");if(data.schemaVersion==3&&(!text.Contains("\"variants\"")||!text.Contains("\"appearances\"")))throw new InvalidDataException("Variantes incompletas.");return new CollectionLibrary(catalog,data);
        }
        public void Save(CollectionLibrary library)
        {
            if(!CanWrite)throw new InvalidOperationException(Warning);
            Directory.CreateDirectory(Path.GetDirectoryName(PathName));string temp=PathName+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(library.Data,true));
            if(File.Exists(PathName))File.Replace(temp,PathName,PathName+".bak");else File.Move(temp,PathName);
        }
    }
}

