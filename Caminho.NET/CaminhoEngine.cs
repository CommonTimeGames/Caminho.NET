using System;
using System.Reflection;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Caminho
{
    public class CaminhoEngine
    {
        public CaminhoStatus Status { get; private set; }
        public CaminhoContext Context { get; private set; }
        public CaminhoNode Current { get; private set; }

        public bool CacheEnabled { get; set; }
        public bool AutoAdvance { get; set; }

        private Script _scriptContext;

        public CaminhoEngine()
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var entryAssembly = Assembly.GetEntryAssembly();

            _scriptContext = new Script();
            _scriptContext.Options.ScriptLoader =
                              new CaminhoScriptLoader();

            var names = assembly.GetManifestResourceNames();
            var entryNames = entryAssembly.GetManifestResourceNames();

            _scriptContext.DoString(BOOTSTRAP);
        }

        public void Start(string name,
                          string package = default(string),
                          string startNode = default(string))
        {
            var arg = new Table(_scriptContext);

            arg["name"] = name;

            if (!string.IsNullOrWhiteSpace(package))
            {
                arg["package"] = package;
            }

            if (!string.IsNullOrWhiteSpace(startNode))
            {
                arg["start"] = startNode;
            }

            var engine = _scriptContext.Globals.Get("c");
            var engineClass = _scriptContext.Globals.Get("Caminho");
            var startMethod = engineClass.Table.Get("Start");
            _scriptContext.Call(startMethod, engine, DynValue.NewTable(arg));

            var e = _scriptContext.Globals.Get("c");
            var text = e.Table.Get("current").Table.Get("node").Table.Get("text");
            Console.WriteLine("First Text Node: " + text);
        }

        public void Continue(int index = -1)
        {

        }

        public void End()
        {

        }

        private DynValue LoadDialogue(string name)
        {
            return null;
        }

        public string BOOTSTRAP
            = @"require('caminho'); c = Caminho:new();";


        public class CaminhoContext
        {
            private CaminhoEngine _engine;

            public CaminhoContext(CaminhoEngine engine)
            {
                _engine = engine;
            }
        }

        public class CaminhoNode
        {
            private CaminhoEngine _engine;

            public string DialogueName { get; private set; }
            public string Package { get; private set; }

            public string Text { get; private set; }
            public string TextKey { get; private set; }

            public string Event { get; set; }


            public CaminhoNode(CaminhoEngine engine)
            {
                _engine = engine;
            }
        }

        public class CaminhoScriptLoader : ScriptLoaderBase
        {
            private Assembly _assembly;
            private string[] _resourceNames;

            public CaminhoScriptLoader()
            {
                _assembly = typeof(CaminhoEngine).GetTypeInfo().Assembly;
                _resourceNames = _assembly.GetManifestResourceNames();
                ModulePaths = new string[] { "?", "?.lua" };
            }

            public override object LoadFile(string file, Table globalContext)
            {
                if (file.Contains("./") || file.Contains(".\\"))
                {
                    file = file.Substring(2);
                }

                Console.WriteLine("LoadFile({0})", file);
                var result = _resourceNames.First(x => x.Contains(file));
                return _assembly.GetManifestResourceStream(result);
            }

            public override bool ScriptFileExists(string name)
            {
                Console.WriteLine("ScriptFileExists({0})", name);
                return _resourceNames.Any(x => x.Contains(name));
            }
        }

    }

    public enum CaminhoStatus
    {
        Inactive,
        Active,
        Error
    }
}
