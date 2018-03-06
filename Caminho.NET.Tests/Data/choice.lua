require('dialogue')

local d = Dialogue:new()

ch={{text="First Choice", next="firstSelected"}, 
    {text="Second Choice", next="secondSelected"}}

d:add{name="choice", 
      text="What do you choose?",
      choices=ch, 
      start=true}
      
d:add{name="firstSelected", text="First Selected!"}
d:add{name="secondSelected", text="Second Selected!"}

return d