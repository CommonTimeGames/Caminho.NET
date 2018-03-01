using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Caminho;

namespace Caminho.Loaders
{
    public class CaminhoEmbeddedEngineLoader : ICaminhoEngineLoader
    {
        private Assembly _assembly;
        private string[] _resourceNames;

        public CaminhoEmbeddedEngineLoader()
        {
            _assembly = typeof(CaminhoEngine).GetTypeInfo().Assembly;
            _resourceNames = _assembly.GetManifestResourceNames();
        }

        bool ICaminhoEngineLoader.Exists(string file)
        {
            return _resourceNames.Any(x => x.Contains(file));
        }

        Stream ICaminhoEngineLoader.LoadFile(string file)
        {
            if (file.Contains("./") || file.Contains(".\\"))
            {
                file = file.Substring(2);
            }

            //Console.WriteLine("LoadFile({0})", file);
            var result = _resourceNames.First(x => x.Contains(file));
            return _assembly.GetManifestResourceStream(result);
        }
    }
}
