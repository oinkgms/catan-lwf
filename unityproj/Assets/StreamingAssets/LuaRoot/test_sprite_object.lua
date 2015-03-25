local t = 0
local function start()
  local g = GameObject.current()
  g.sprite_object:play("melee");
end

local function update()
  t = t + delta_time()
  if t > 10 then
    local g = GameObject.current()
    g.sprite_object:play("idle");

  end
end

local function coroutine()




end

return {
  start = start,
  update = update,
  coroutine = coroutine,
}
