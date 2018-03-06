require('dialogue')

local d = Dialogue:new()

d:add{name="a", text="First", next="b", start=true}
d:add{name="b", text="Second", next="c"}
d:add{name="c", text="Third"}

d:add{package="second", name="a", text="Second:First", next="b", start=true}
d:add{package="second", name="b", text="Second:Second", next="c"}
d:add{package="second", name="c", text="Second:Third"}

d:seq{
  package="third",
  start=true,
  name="test",
  {text="Sequence1"},
  {text="Sequence2"},
  {text="Sequence3"}
}

d:add{package="fourth", name="a", text="First", next="b", start=true}
d:add{package="fourth", name="b", key="fourth.key", next="c"}
d:add{package="fourth", name="c", text="textFunc"}

d.fourth.textFunc = function(d)
  return "FunctionText"
end



return d