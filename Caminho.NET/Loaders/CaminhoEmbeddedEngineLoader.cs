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
#if UNITY_5_3_OR_NEWER
            _assembly = typeof(CaminhoEngine).Assembly;
#else
            _assembly = typeof(CaminhoEngine).GetTypeInfo().Assembly;
#endif
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
