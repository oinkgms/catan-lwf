using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

namespace catan {
  public class LuaThread : LuaClassCommon {
    public enum Phase {
      NONE,
      LOADED,
      PRERUN,
      RUNNING,
      ERROR,
      EXIT,
    };
    private static string KlassName = "lthread";
    private ILuaState _luaState;
    private Phase _phase;
    private int _narg;
    //
    public ILuaState luaState {
      get {
        return _luaState;
      }
    }
    public Phase phase {
      get {
        return _phase;
      }
    }
    public ThreadStatus status {
      get {
        return _luaState.Status;
      }
    }
    public bool isLoaded {
      get {
        return _phase == Phase.LOADED;
      }
    }
    public bool isRunning {
      get {
        return _phase == Phase.RUNNING || _phase == Phase.PRERUN;
      }
    }
    public bool isExit {
      get {
        return _phase == Phase.EXIT;
      }
    }
    public bool isError {
      get {
        return this.status != ThreadStatus.LUA_OK && this.status != ThreadStatus.LUA_YIELD;
      }
    }
    //
    public static void LoadLibs(ILuaState lua) {
      int n = lua.GetTop();
      lua.NewTable();
      lua.PushValue(-1);
      lua.SetGlobal(KlassName);
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("current", L_lthread_current),
        new NameFuncPair("start", L_lthread_start),
        // instance
        new NameFuncPair("is_alive", L_lthread_is_alive),
        
      };
      lua.L_SetFuncs(mfuncs, 0);
      lua.SetTop(n);
    }
    //
    private static int NewLuaThread(ILuaState lua, LuaThread th) {
      // o = {}
      lua.NewTable();
      // setmetatable(o, self)
      //lua.PushValue(-2); // self
      lua.GetGlobal(KlassName);
      lua.SetMetaTable(-2); // -> o
      // self.__index = self
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      // return o
      if (th != null) {
        set_internal<LuaThread>(lua, -2, th);
      }
      return 1;
    }
    // @class lthread
    // @internal
    // @desc 現在実行しているカレントのスレッドを取得する
    // @result カレントスレッド
    private static int L_lthread_current(ILuaState lua) {
#if false
      NewLuaThread(lua, GetThreadFromMeta(lua));
      return 1;
#else
      return 0;
#endif
    }
    // @desc スレッドを開始する
    // @param function スレッドで実行する関数
    // @result スレッドインスタンス
    // @sample local th = lthread.start(function() print("HOGE") end)
    private static int L_lthread_start(ILuaState lua) {
      LuaScriptBehaviour lsb = LuaScriptBehaviour.FindFromState(lua);
      LuaThread th = lsb.StartThread(lua);
      NewLuaThread(lua, th);
      return 1;
    }
    // @desc スレッドが生きているかどうか確認する
    // @param self スレッドインスタンス
    // @result 生死の真偽値
    // @sample if th:is_alive() then
    // @sample   print("ALIVE!")
    // @sample end
    private static int L_lthread_is_alive(ILuaState lua) {
      LuaThread th = get_internal<LuaThread>(lua, 1);
      lua.PushBoolean(!th.isExit);
      return 1;
    }
    //
    public LuaThread(ILuaState lua, LuaScriptBehaviour behaviour) {
      lua.NewThread();
      _luaState = lua.ToThread(-1);
      lua.Pop(1);
      _phase = Phase.NONE;
    }
    public void Start(int num) {
      _narg = num;
      _phase = Phase.PRERUN;
    }
    public void Resume() {
      if (_phase == Phase.PRERUN) {
        _luaState.Resume(_luaState, _narg);
        if (this.isError) {
          Debug.LogError(_luaState.ToString(-1));
          _phase = Phase.ERROR;
        } else {
          _phase = Phase.RUNNING;
        }
      } else
      if (_phase == Phase.RUNNING) {
        _luaState.Resume(_luaState, 0);
        if (this.isError) {
          Debug.LogError(_luaState.ToString(-1));
          _phase = Phase.ERROR;
        }
      }
      if (this.status == ThreadStatus.LUA_OK || this.status != ThreadStatus.LUA_YIELD) {
        _phase = Phase.EXIT;
      }
    }
  };
}
