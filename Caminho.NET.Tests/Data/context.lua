require('dialogue')

local d = Dialogue:new()

d:add{name="a", set="test", value=5, next="b", start=true}
d:add{name="b", text="Second", next="c"}

d:add{package="second", name="a", set="test", value="foo", next="b", start=true}
d:add{package="second", name="b", text="Second", next="c"}

d:add{package="third", name="a", set="test", value="5", next="b", start=true}
d:add{package="third", name="b", increment="test", next="c"}
d:add{package="third", name="c", text="Second"}

d:add{package="fourth", name="a", func="d.context.test = 8", next="b", start=true}
d:add{package="fourth", name="b", decrement="test", next="c"}
d:add{package="fourth", name="c", text="Second"}

return d