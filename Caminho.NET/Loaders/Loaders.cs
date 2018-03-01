using System;
using System.IO;
namespace Caminho.Loaders
{
    public interface ICaminhoEngineLoader
    {
        Stream LoadFile(string file);
        bool Exists(string file);
    }

    public interface ICaminhoDialogueLoader
    {
        Stream LoadDialogue(string file);
    }

    public interface ICaminhoTextLoader
    {
        string LoadText(string key);
    }
}
