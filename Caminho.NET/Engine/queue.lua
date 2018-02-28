-- Adapted from Programming in Lua (first edition)
-- by Roberto Ierusalimschy 
-- Lua.org, December 2003 
-- ISBN 8590379817

Queue={}

function Queue:new()
    local o = {first=0, last=-1, size=0}
    o.keys={}
    o.indices={}
    setmetatable(o, self)
    return o
end

function Queue:saveKey(key, index)
    self.keys[key] = index
    self.indices[index] = key
end

function Queue:evictKey(index)
    local k = self.indices[index]
    if k then
        self.keys[k] = nil
        self.indices[index] = nil
    end
end

function Queue:find(key)
    if not key then return end
    return self[self.keys[key]]
end

Queue.__index = function(table, key)
    local mt = getmetatable(table)
    if mt[key] then return mt[key] end
    return Queue.find(table, key)
end

function Queue:pushLeft(value,key)
    local first = self.first - 1
    self.first = first
    self[first] = value
    self.size = self.size + 1
    if key then self:saveKey(key, first) end
end

function Queue:pushRight(value, key)
    local last = self.last + 1
    self.last = last
    self[last] = value
    self.size = self.size + 1
    if key then self:saveKey(key, last) end
end

function Queue:popLeft()
    local first = self.first
    if first > self.last then error("Queue is empty!") end
    local value = self[first]
    self[first] = nil
    self:evictKey(first)
    self.first = first + 1
    self.size = self.size - 1
    return value
end

function Queue:popRight()
    local last = self.last
    if self.first > last then error ("Queue is empty!") end
    local value = self[last]
    self[last] = nil
    self:evictKey(last)
    self.last = last - 1
    self.size = self.size - 1
    return value
end