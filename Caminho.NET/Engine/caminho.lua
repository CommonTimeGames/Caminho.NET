require('queue')

Caminho = {}

function Caminho:new(o)
  o = o or {}
  setmetatable(o, self)
  self.__index = self
  
  o.loader = Caminho.Load
  o.loadDir = "."

  o.cacheEnabled = true
  o.cacheMaxSize = 10
  o.cache = Queue:new()

  o.autoAdvance = true
  
  o.status = "inactive"
  o.locale = "en"
  o.context = o.context or {}

  return o
end

function Caminho:Load(filename)
  local path = self.loadDir .. "/" .. filename
  return assert(loadfile(path))
end

function Caminho:Run()
  self.status = "active"

  if self.autoAdvance and self.current.node.autoAdvance then
    repeat
      self.current.node:OnEnter(self.current)
      self:Advance()
    until not self.current.node or not self.current.node.autoAdvance
  end

  while self.current.node do
    self.current.node:OnEnter(self.current)
    local n = coroutine.yield()
    self:Advance(n)
  end

  self.current = nil
  self.status = "inactive"
end

function Caminho:Advance(n)
  if self.autoAdvance then
    repeat
      self.current.node = self.current.node:Next(self.current, n)
    until not self.current.node or not self.current.node.autoAdvance
  else
    self.current.node = self.current.node:Next(self.current, n)
  end
end

function Caminho:setError(err)
  self.status = "error"
  self.current = self.current or {}
  self.current.error = err
end

function Caminho:loadFromCache(name)

  if self.cache[name] then
    return self.cache[name]

  else
    local d = self:loader(name)

    if not d then return end

    while self.cache.size >= self.cacheMaxSize do
      self.cache:popRight()
    end

    self.cache:pushLeft(d, name)

    return d
  end
end

function Caminho:Start(arg)
  assert(self, "Call Caminho:Start(), not Caminho.Start()!")
  status, err = pcall(function()
      assert(arg and arg.name, "Caminho:Start(): A valid dialogue name must be specified!")
      assert(self.loader, "Caminho:Start(): A valid dialogue loader must be provided!")
      
      local d = nil
      
      if self.cacheEnabled then
        d = self:loadFromCache(arg.name)
        --d = self:loader(arg.name)

      else
        d = self:loader(arg.name)
      end

      assert(d, "Caminho:Start(): Cannot find dialogue: " .. arg.name)

      d = d()

      assert(d and type(d) == "table",
       "Caminho:Start(): Dialogue: " .. arg.name .. " must return a table (see example files)!")

      local package = arg.package or "default"
      local data = d[package]

      assert(data,
       "Dialogue: " .. arg.name .. " is missing package " .. package .. "!")

      local startNode = data[arg.start] or data[data.start]

      assert(startNode,
       "Dialogue: " .. arg.name .. ", package: " .. package ..
        ": Unable to find start node " .. (arg.start or "start"))

      self.current ={
        name=arg.name,
        package=arg.package,
        data=data,
        context=self.context,
        node=startNode,
        co=coroutine.create(Caminho.Run)
      }

      coSuccess, coError = coroutine.resume(self.current.co, self)

      if not coSuccess then
        error(coError)
      end

    end
  )

  if not status then
    self:setError(err)
    error(err)
  end

end

function Caminho:Continue(val)
  assert(self, "Call Caminho:Continue(), not Caminho.Continue()!")
  assert(self.status == "active", "Caminho:Continue(): No dialogue is currently active.")
  status, err = coroutine.resume(self.current.co, val)
  
  if not status then
    self:setError(err)
    error(err)
  end

end

function Caminho:End()
  assert(self, "Call Caminho:End(), not Caminho.End()!")
  self.current = nil
  self.status = "inactive"
end

