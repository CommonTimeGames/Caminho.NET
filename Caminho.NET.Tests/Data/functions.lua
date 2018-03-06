require('dialogue')

local d = Dialogue:new()

d:add{name="func",func=function(d) d.context.test = 3 end, start=true}

d.second = {}
d.second.test_func = function(d)
  d.context.test = 4
end

d:add{package="second", name="func", func="test", start=true}

d:add{package="third", name="func", func="d.context.test = 5", start=true}

return d