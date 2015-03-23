using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniLua;

namespace catan {
  public class LuaGameObjectCache : MonoBehaviour {
    private int _index = LuaConstants.LUA_NOREF;
    private ILuaState _luaState;
    public int index {
      get { return _index; }
      set { _index = value; }
    }
    public ILuaState luaState {
      get { return _luaState; }
      set { _luaState = value; }
    }
    void OnDestroy() {
      if (_index != LuaConstants.LUA_NOREF) _luaState.L_Unref(LuaDef.LUA_REGISTRYINDEX, _index);
    }
  }
}
