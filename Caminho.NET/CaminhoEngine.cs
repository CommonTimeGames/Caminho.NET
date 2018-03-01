using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Caminho.Loaders;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Caminho
{
    public class CaminhoEngine
    {
        public CaminhoStatus Status
        {
            get
            {
                var engine = _scriptContext.Globals.Get("c");
                var status = engine.Table.Get("status");
                return FromString(status.String);
            }
        }

        public CaminhoContext Context { get; private set; }
        public CaminhoNode Current { get; private set; }

        public bool CacheEnabled
        {
            get
            {
                var engine = _scriptContext.Globals.Get("c");
                var cacheEnabled = engine.Table.Get("cacheEnabled");
                return cacheEnabled.Boolean;
            }
            set
            {
                var engine = _scriptContext.Globals.Get("c");
                engine.Table["cacheEnabled"] = value;
            }
        }

        public bool AutoAdvance
        {
            get
            {
                var engine = _scriptContext.Globals.Get("c");
                var cacheEnabled = engine.Table.Get("autoAdvance");
                return cacheEnabled.Boolean;
            }
            set
            {
                var engine = _scriptContext.Globals.Get("c");
                engine.Table["autoAdvance"] = value;
            }
        }

        public ICaminhoEngineLoader EngineLoader;
        public ICaminhoDialogueLoader DialogueLoader;
        public ICaminhoTextLoader TextLoader;

        private Script _scriptContext;

        public CaminhoEngine()
        {
            DialogueLoader = new CaminhoFileSystemDialogueLoader();
            EngineLoader = new CaminhoEmbeddedEngineLoader();

            _scriptContext = new Script();
            _scriptContext.Options.ScriptLoader =
                              new CaminhoScriptLoader(this);

            Context = new CaminhoContext(this);
            Current = new CaminhoNode(this);
        }

        public void Initialize()
        {
            _scriptContext.DoString(BOOTSTRAP);
            var engine = _scriptContext.Globals.Get("c");
            engine.Table["loader"] = (Func<string, DynValue>)LoadDialogue;
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

            Current.SyncCurrent();
        }

        public void Continue(int index = 0)
        {
            var engine = _scriptContext.Globals.Get("c");
            var engineClass = _scriptContext.Globals.Get("Caminho");
            var continueMethod = engineClass.Table.Get("Continue");

            if (index > 0)
            {
                _scriptContext.Call(continueMethod, engine, index);
            }
            else
            {
                _scriptContext.Call(continueMethod, engine);
            }

            Current.SyncCurrent();
        }

        public void End()
        {
            var engine = _scriptContext.Globals.Get("c");
            var engineClass = _scriptContext.Globals.Get("Caminho");
            var endMethod = engineClass.Table.Get("End");
            _scriptContext.Call(endMethod, engine);
        }

        private DynValue LoadDialogue(string name)
        {
            var scriptStream = DialogueLoader.LoadDialogue(name);

            return scriptStream != null ?
                _scriptContext.LoadStream(scriptStream) : DynValue.Nil;
        }

        public string BOOTSTRAP
            = @"require('caminho'); c = Caminho:new();";


        public class CaminhoContext
        {
            private CaminhoEngine _engine;

            internal CaminhoContext(CaminhoEngine engine)
            {
                _engine = engine;
            }

            public object this[object key]
            {
                get
                {
                    var engine = _engine._scriptContext.Globals.Get("c");
                    var context = engine.Table.Get("context");
                    return context.Table.Get(key).ToObject();
                }
                set
                {
                    var engine = _engine._scriptContext.Globals.Get("c");
                    var context = engine.Table.Get("context");
                    context.Table.Set(key,
                                      DynValue.FromObject(_engine._scriptContext,
                                                          value));
                }
            }
        }

        public class CaminhoNode
        {
            private CaminhoEngine _engine;

            public string DialogueName { get; private set; }
            public string Package { get; private set; }

            public CaminhoNodeType Type { get; private set; }

            public string Next { get; private set; }

            public string Text { get; private set; }
            public string TextKey { get; private set; }

            public string Event { get; private set; }
            public Dictionary<object, object> EventData { get; private set; }

            public double WaitTime { get; private set; }

            public string FunctionName { get; private set; }

            public string ContextVariable { get; private set; }
            public string ContextValue { get; private set; }

            public string ErrorMessage { get; private set; }

            internal CaminhoNode(CaminhoEngine engine)
            {
                _engine = engine;
            }

            public void SyncCurrent()
            {
                ClearCurrent();

                var c = _engine._scriptContext.Globals.Get("c");

                var current = c.Table.Get("current");
                if (current.IsNil()) { return; }

                var node = current.Table.Get("node");
                if (node == null) { return; }

                Type = FromString(node.Table.MetaTable.Get("type"));

                switch (Type)
                {
                    case CaminhoNodeType.Text:
                    case CaminhoNodeType.Choice:
                        var text = node.Table.Get("text");
                        var key = node.Table.Get("key");
                        this.Text = text.IsNotNil() ? text.String : null;
                        this.TextKey = key.IsNotNil() ? key.String : null;
                        break;

                    case CaminhoNodeType.Wait:
                        var wait = node.Table.Get("wait");
                        this.WaitTime = wait.IsNotNil() ? wait.Number : -1;
                        break;

                    case CaminhoNodeType.Event:
                        var ev = node.Table.Get("event");
                        this.Event = ev.IsNotNil() ? ev.String : null;
                        break;

                    case CaminhoNodeType.Function:
                        var fn = node.Table.Get("funcName");
                        this.Event = fn.IsNotNil() ? fn.String : null;
                        break;

                    case CaminhoNodeType.Set:
                    case CaminhoNodeType.Increment:
                    case CaminhoNodeType.Decrement:
                        var st = node.Table.Get("set");
                        var val = node.Table.Get("value");
                        this.ContextVariable = st.IsNotNil() ? st.String : null;
                        this.ContextValue = st.IsNotNil() ? val.CastToString() : null;
                        break;

                    case CaminhoNodeType.Error:
                        var err = current.Table.Get("error");
                        this.ErrorMessage = err.IsNotNil() ? err.String : null;
                        break;
                }
            }

            private void ClearCurrent()
            {
                DialogueName = null;
                Package = null;
                Type = CaminhoNodeType.Error;
                Next = null;
                Text = null;
                TextKey = null;
                Event = null;
                EventData = null;
                WaitTime = -1;
                FunctionName = null;
                ContextVariable = null;
                ContextValue = null;
                ErrorMessage = null;
            }

            private CaminhoNodeType FromString(DynValue val)
            {
                if (val.IsNil())
                {
                    return CaminhoNodeType.Error;
                }
                else if (val.String == "text")
                {
                    return CaminhoNodeType.Text;
                }
                else if (val.String == "choice")
                {
                    return CaminhoNodeType.Choice;
                }
                else if (val.String == "wait")
                {
                    return CaminhoNodeType.Choice;
                }
                else if (val.String == "event")
                {
                    return CaminhoNodeType.Choice;
                }
                else if (val.String == "function")
                {
                    return CaminhoNodeType.Function;
                }
                else if (val.String == "set")
                {
                    return CaminhoNodeType.Set;
                }
                else if (val.String == "increment")
                {
                    return CaminhoNodeType.Increment;
                }
                else if (val.String == "decrement")
                {
                    return CaminhoNodeType.Increment;
                }
                else if (val.String == "conditional")
                {
                    return CaminhoNodeType.Error;
                }

                return CaminhoNodeType.Error;
            }
        }

        private static CaminhoStatus FromString(string val)
        {

            if (val == "inactive")
            {
                return CaminhoStatus.Inactive;
            }
            else if (val == "active")
            {
                return CaminhoStatus.Active;
            }
            else
            {
                return CaminhoStatus.Error;
            }
        }
    }



    public enum CaminhoStatus
    {
        Inactive,
        Active,
        Error
    }

    public enum CaminhoNodeType
    {
        Text,
        Choice,
        Wait,
        Event,
        Function,
        Set,
        Increment,
        Decrement,
        Conditional,
        Error
    }
}
