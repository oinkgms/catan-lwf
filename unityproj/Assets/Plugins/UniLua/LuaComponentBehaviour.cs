using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniLua;

namespace catan {
  public class LuaComponentBehaviour : LuaClassCommonUnityObject<LuaScriptBehaviour> {
    private static string klassName = "Behaviour";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        new NameFuncPair("current", L_current),
        new NameFuncPair("get_filename", L_get_filename),
        new NameFuncPair("set_filename", L_set_filename),
        new NameFuncPair("reload_functions", L_reload_functions),
      };
      register_class(lua, klassName, mfuncs);
    }
    public static int PushNew(ILuaState lua, LuaScriptBehaviour lsb) {
      return push_new<LuaScriptBehaviour>(lua, klassName, lsb, lsb.gameObject);
    }
    // @class Behaviour
    // @desc このスクリプトが動いているBehaviourを取得する
    // @decl Behaviour current()
    // @result Behaviour
    // @sample local sb = Behaviour.current()
    private static int L_current(ILuaState lua) {
      LuaScriptBehaviour lsb = LuaScriptBehaviour.FindFromState(lua);
      PushNew(lua, lsb);
      return 1;
    }
    // @desc 実行しているLuaスクリプトファイル名を得る
    // @decl string get_filename(self)
    // @result ファイル名
    // @sample local filename = GameObject.current.behaviour:get_filename()
    private static int L_get_filename(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      string filename = get_internal<LuaScriptBehaviour>(lua, 1).luaScriptFile;
      lua.PushString(filename);
      return 1;
    }
    // @desc 実行したいLuaスクリプトファイル名を設定する
    // @decl set_filename(self, filename)
    // @param filename ファイル名
    // @note セットするだけで実行されます
    // @note ただし、既に何かが実行されている場合は無効
    // @sample GameObject.find("HOGE").behaviour:set_filename("catan/hoge.lua")
    private static int L_set_filename(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      string filename = lua.ToString(2);
      get_internal<LuaScriptBehaviour>(lua, 1).luaScriptFile = filename;
      return 0;
    }
    // @desc セルフテーブルから関数をリロードします
    // @decl reload_functions(self)
    // @sample local function start(self)
    // @sample   self.on_click = function() print("hoge") end
    // @sample   GameObject.current().behaviour:reload_functions() -- <-- これ以降、on_clickが外部から呼べるようになります
    // @sample end
    private static int L_reload_functions(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<LuaScriptBehaviour>(lua, 1).AppendFromSelfTable();
      return 0;
    }
  }
}

