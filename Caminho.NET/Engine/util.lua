util = {}

local charset = {}  do -- [0-9a-zA-Z]
  for c = 48, 57  do table.insert(charset, string.char(c)) end
  for c = 65, 90  do table.insert(charset, string.char(c)) end
  for c = 97, 122 do table.insert(charset, string.char(c)) end
end

function util.randomString(length)
  if not length or length <= 0 then return '' end
  return util.randomString(length - 1) .. charset[math.random(1, #charset)]
end

function util.map(func, array)
  local new_array = {}
  for i,v in ipairs(array) do
    new_array[i] = func(v)
  end
  return new_array
end

