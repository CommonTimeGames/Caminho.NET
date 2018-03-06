require('util')

-- Base Node class
Node = {autoAdvance=false}

function Node:new(o)
    o = o or {}
	setmetatable(o, self)
    self.__index = self
    return o
end

function Node:OnEnter(d) end

function Node:Next(d)
    if self.next then
        return d.data[self.next]
    end
end

-- TextNode 
TextNode = Node:new{type = "text"}

function TextNode:OnEnter(d) 
    self.displayText = self:GetText(d)
end

function TextNode:GetText(d)

    if self.key then

        -- TODO: Look up string for current locale 
        return self.key

    elseif type(d.data[self.text]) == "function" then

        -- If text points to a function in d.func, 
        -- then return the result of that call

        local ret = d.data[self.text](d)

        if ret and type(ret) == "string" then
            return ret
        else
            error("TextNode.GetText() function " .. self.text .. " must return a string!")
            return nil
        end

    elseif self.text and type(self.text) == "string" then
        -- If text property is a string, return that string
        return self.text
    end

end

-- WaitNode
WaitNode = Node:new{type = "wait", time = 3}

function WaitNode:Update(time)
    self.elapsed = self.elapsed or 0
    self.elapsed = self.elapsed + time
end

-- FunctionNode
FunctionNode = Node:new{type = "function", autoAdvance=true}

function FunctionNode:Next(d)

    local funcSuffix = "_func"
    local targetFunc = nil
    
    -- FunctionNode must have property "func" defined;
    -- Then a function matching that name is searched for in
    -- the dialogue table. No globals function calls are allowed.

    if not self.func then
        error("FunctionNode: func property is missing!")
        return nil

    else
        local funcName = self.func .. funcSuffix

        assert(d.data[funcName],
         "FunctionNode:Next() Missing function declaration " .. funcName)

        assert(type(d.data[funcName]) == "function",
             "FunctionNode:Next() " .. funcName .. " must be callable!")

        targetFunc = d.data[funcName]

    end

    local ret = targetFunc(d)
        
    -- If function returns no value, then 
    -- attempt to use value self.next
    
    if not ret and self.next then
        return d.data[self.next]

    -- If function returns a table,
    -- this becomes the active node
    elseif ret and type(ret) == "table" then
        return ret

    -- Otherwise perform lookup to get
    -- the next node
    else
        return d.data[ret]

    end
end

-- ChoiceNode
ChoiceNode = TextNode:new{type = "choice"}

function ChoiceNode:Next(d, v)

    assert(self.choices and type(self.choices) == "table",
        "ChoiceNode.choices must be a table")

    v = tonumber(v) or 1

    return self.choices[v]:Next(d)

end

-- EventNode
EventNode = Node:new{type = "event"}

-- ErrorNode
ErrorNode = Node:new{type="error"}

-- SetContextNode

SetContextNode = Node:new{type="set", autoAdvance=true}

function SetContextNode:Next(d)
    assert(self.set,
     "SetContextNode:Next(): Property 'set' must be present (context key to be set)!")

    assert(self.value,
        "SetContextNode:Next(): Property 'value' must be present (value to be set)!")

    local skip = self.onceKey and d.context[self.onceKey] == true

    if not skip then
        d.context[self.set] = self.value
        if self.onceKey then d.context[self.onceKey] = true end
    end

    return Node.Next(self, d)

end

IncrementNode = Node:new{type="increment", autoAdvance=true}

function IncrementNode:Next(d)
    assert(self.increment,
     "IncrementNode:Next(): Property 'increment' must be present (context key to be incremented)!")

    local value = d.context[self.increment] or 0

    assert(tonumber(value),
        "IncrementNode:Next(): Context property '" .. self.increment .. " cannot be incremented!")
    
    self.set = self.increment
    self.value = value + 1

    return SetContextNode.Next(self, d)
end

DecrementNode = Node:new{type="decrement", autoAdvance=true}

function DecrementNode:Next(d)
    assert(self.decrement,
     "DecrementNode:Next(): Property 'decrement' must be present (context key to be decremented)!")

    local value = d.context[self.decrement] or 0

    assert(tonumber(value),
        "DecrementNode:Next(): Context property '" .. self.decrement .. "' cannot be decremented!")
    
    self.set = self.decrement
    self.value = tonumber(value) - 1

    return SetContextNode.Next(self, d)
end


-- Dialogue 
Dialogue = {}

function Dialogue:new(o)
    o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function Dialogue:makeFunction(arg)
    local package = arg.package or "default"
    local funcSuffix = "_func"

    self[package] = self[package] or {}

    if type(arg.func) == "string" then
        local funcName = arg.func .. funcSuffix

        if type(self[package][funcName]) == "function" then
            return FunctionNode:new{func=arg.func}
        else
            local snippetContext = "local d = ... ; "
            local compiled = 
                assert(load(snippetContext .. arg.func),
                 "Dialogue:makeFunction(): '" 
                    .. arg.func .. "' is not a valid existing function or valid Lua snippet.")

            local baseName = util.randomString(5)
            local funcName = baseName .. funcSuffix

            self[package][funcName] = function(d) compiled(d) end
            
            return FunctionNode:new{func=baseName}
        end

    elseif type(arg.func) == "function" then

        local baseName = arg.name or arg.funcName or util.randomString(5)
        local funcName = baseName .. funcSuffix
        self[package][funcName] = arg.func
        return FunctionNode:new{func=baseName}

    else
        error("Dialogue:makeFunction(): arg.func " 
            .. arg.func .. " must be callable or the name of a callable")
    end
    
end

function Dialogue:getNode(arg)
        
    if arg.node then
        return node
    
    elseif arg.choices then
        return ChoiceNode:new{
            text=arg.text,
            key=arg.key,
            choices=util.map(function(n) return TextNode:new(n) end, arg.choices)
        }

    elseif arg.func then
        return self:makeFunction(arg)
        
    elseif arg.event then
        return EventNode:new(arg)

    elseif arg.wait then
        return WaitNode:new(arg)

    elseif arg.set then
        return SetContextNode:new(arg)

    elseif arg.increment then
        return IncrementNode:new(arg)

    elseif arg.decrement then
        return DecrementNode:new(arg)

    elseif arg.text or arg.key then
        return TextNode:new(arg)

    end

end

function Dialogue:add(arg)

    assert(arg, "Dialogue:add(): Missing argument!")
    assert(arg.name, "Dialogue:add(): Missing node name!")

    local package = arg.package or "default"
    self[package] = self[package] or {}

    local node = self:getNode(arg)
    assert(node, "Dialogue:add(): Could not find or deduce a valid node!")

    node.next = arg.next

    self[package][arg.name] = node

    if arg.start then
        self[package].start = arg.name
    end

end

function Dialogue:func(arg, func)

    assert(arg, "Dialogue:func(): Missing first argument!")
    assert(arg.name, "Dialogue:add(): Missing node name!")
    assert(func and type(func) == "function",
         "Dialogue:func(): func must be callable")

    local package = arg.package or "default"
    self[package] = self[package] or {}

    local funcName = arg.name .. "_func"
    self[package][funcName] = func

    local node = FunctionNode:new{func=arg.name, next=arg.next}
    self[package][arg.name] = node

end

function Dialogue:sequence(arg)
    assert(arg, "Dialogue:sequence(): Missing first argument!")
    assert(arg.name, "Dialogue:sequence(): arg.name property required!")
    assert(#arg > 0, "Dialogue:sequence(): At least one element required!")

    local package = arg.package or "default"
    self[package] = self[package] or {}

    local suffix = "_seq_"

    for i,v in ipairs(arg) do
        arg[i].package = package
        local node = self:getNode(arg[i])

        assert(node, "Dialogue:sequence(): Could not deduce node #" .. i)
        
        assert(node.type ~= ChoiceNode.type,
         "Dialogue:sequence(): Choice nodes are not allowed in sequences!")

        local destKey = ""
        local nextKey = ""

        destKey = arg.name

        if i > 1 then 
            destKey = destKey .. suffix .. i
        end

        if i == #arg then
            nextKey = arg.next
        else
            nextKey = arg.name .. suffix .. i + 1
        end

        node.next = nextKey

        self[package][destKey] = node
    end

    if arg.start then
        self[package].start = arg.name
    end
end

Dialogue.seq = Dialogue.sequence


