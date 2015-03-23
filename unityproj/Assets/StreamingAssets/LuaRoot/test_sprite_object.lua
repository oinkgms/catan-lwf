local function start()
  print("HOGEgGEGEGE")
end

local function update()
  local g = GameObject.current()
  print(g.lwf)
end



return {
  start = start,
  update = update,
}
