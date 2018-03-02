# Caminho.NET #

Caminho is a flexible, lightweight dialogue engine for games. 
Written in Lua, it embeds easily into your game engine of choice,
and allows you to focus on creating great content, not boilerplate code.

### Features ###

* Decouple dialogue content and logic from your game code.
* Iterate dialogue content without recompiling.
* Write dialogues with a simple syntax.
* Easily script complicated behaviors in your dialogues with Lua.
* Test and debug dialogues outside your game!

### Platforms ###

These .NET bindings let you easily use Caminho in a .NET environment (such as MonoGame).

The .NET version of Caminho uses the MoonSharp Lua Interpreter to run the engine code on the CLR.
It strikes a good balance between performance and interoperability.

If you’re using Unity, take a look at the Caminho-Unity project instead.

### Requirements ###

* .NET Standard 1.6 or later (.NET Framework/Mono/.NET Core)
* MoonSharp 2.0 or later (automatically installed as a dependency)

### Installation ###

A NuGet package is coming soon. Until then, you can add this code as part of your solution.

### Quickstart ###

Let’s write a sample dialogue:

    “Hello, World!” 
    -> “Welcome to Caminho!” 
      -> “Have a nice day!”

#### hello.lua ####

    require('dialogue')

    local d = Dialogue:new()`

    d:sequence{
       start=true,
       name="test",
       {text="Hello, World!"},
       {text="Welcome to Caminho!"},
       {text="Have a nice day!"}
     }

    return d

Now let's run our dialogue:

    using Caminho;

    public static void Main(string [] args)
    {
	    var engine = new CaminhoEngine()
	    engine.Initialize();
	
	    /* Start the dialogue */
	    engine.start(“helloWorld”);
	
	    Console.WriteLine(engine.current.Text); //“Hello, World!”
	
	    engine.Continue();
	
	    Console.WriteLine(engine.current.Text); //“Welcome to Caminho!”
	
	    engine.Continue();
	
	    Console.WriteLine(engine.current.Text); //“Have a nice day!”
    }

For more information about how Caminho works, take a look at the wiki.

### License ###

Caminho and its related projects are available under the MIT license.