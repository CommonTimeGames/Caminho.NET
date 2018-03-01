using System;
using System.IO;

namespace Caminho.Loaders
{
    public class CaminhoFileSystemDialogueLoader : ICaminhoDialogueLoader
    {
        public string[] SearchPaths { get; set; }

        public CaminhoFileSystemDialogueLoader()
        {
            SearchPaths = new string[] { "?", "?.lua" };
        }

        public Stream LoadDialogue(string file)
        {
            if (SearchPaths == null || SearchPaths.Length == 0)
            {
                return File.Exists(file)
                           ? new FileStream(file, FileMode.Open) : null;
            }

            foreach (var p in SearchPaths)
            {
                var t = p.Replace("?", file);

                if (File.Exists(t))
                {
                    return new FileStream(t, FileMode.Open);
                }
            }

            return null;
        }
    }
}
