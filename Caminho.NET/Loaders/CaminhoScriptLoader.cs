using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Caminho.Loaders
{
    public class CaminhoScriptLoader : ScriptLoaderBase
    {
        private CaminhoEngine _engine;

        public CaminhoScriptLoader(CaminhoEngine engine)
        {
            _engine = engine;
            ModulePaths = new string[] { "?", "?.lua" };
        }

        public override object LoadFile(string file, Table globalContext)
        {
            return _engine.EngineLoader.LoadFile(file);
        }

        public override bool ScriptFileExists(string name)
        {
            return _engine.EngineLoader.Exists(name);
        }
    }
}
