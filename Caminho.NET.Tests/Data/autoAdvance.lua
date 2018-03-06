require('dialogue')

local d = Dialogue:new()

d:seq{
  start=true,
  name="test",
  {func="d.context.foo = 5"},
  {increment="foo"},
  {increment="foo"},
  {text="Stop here."}
}

d:seq{
  start=true,
  package="second",
  name="test",
  {text="Start here."},
  {func="d.context.foo = 5"},
  {increment="foo"},
  {increment="foo"},
  {text="Stop here."}
}

return d