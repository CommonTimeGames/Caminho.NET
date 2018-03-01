require('dialogue')

local d = Dialogue:new()

d:seq{
  start=true,
  name="test",
  next="testFunc",
  {text="Welcome to Caminho.NET"},
  {text="If you're reading this then the dialogue loaded successfully."},
  {text="Good bye!"}
}

return d