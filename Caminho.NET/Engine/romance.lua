require('dialogue')

local introText = "You see a beautiful woman. "
local positiveText = "She smiles brightly at you!"
local neutralText = "She ignores you."
local negativeText = "She sneers at you."
local endText = "You walk away, and wonder what could have been..."

local maxFeeling = 3
local minFeeling = -3

local introChoices=
{
  TextNode:new{text="Give her a flower", next="giveFlowerFunc"},
  TextNode:new{text="Give her a frog", next="giveFrogFunc"},
  TextNode:new{text="Walk away", next="finished"}
}

local data =
{
  default={
    start="intro",
    intro=ChoiceNode:new{text="getIntroText", choices=introChoices},
    giveFlowerFunc=FunctionNode:new{func="giveFlower", next="intro"},
    giveFrogFunc=FunctionNode:new{func="giveFrog", next="intro"},

    getIntroText=function(d)
      local feelingText = ""
      local feeling = d.context.feeling or 0

      if feeling > 2 then
        feelingText = positiveText
      elseif feeling > -1 then
        feelingText = neutralText
      else
        feelingText = negativeText
      end

      return introText .. feelingText

    end,

    giveFlower=function(d)
      d.context.feeling = d.context.feeling or 0
      d.context.feeling = math.min(d.context.feeling + 1, maxFeeling)
      return "intro"
    end,

    giveFrog=function(d)
      d.context.feeling = d.context.feeling or 0
      d.context.feeling = math.max(d.context.feeling - 1, minFeeling)
      return "intro"
    end,

    finished=TextNode:new{text=endText}
  }
}

return data