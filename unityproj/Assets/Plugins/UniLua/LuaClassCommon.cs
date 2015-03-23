using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using UniLua;

namespace catan {
  public class LuaClassCommon {
    protected struct NameNumberPair {
      public string name;
      public double number;
      public NameNumberPair(string n, double num) {
        name = n;
        number = num;
      }
    }
    private static string IdentifierKey = "identifier__";
    private static string InternalKey = "internal__";
    private static string GameObjectKey = "gameobject__";
    // 
    protected static bool get_boolean(ILuaState lua, int idx, string key) {
      idx = LuaVM.NormalizeIndex(lua, idx);
      lua.GetField(idx, key);
      bool b = lua.ToBoolean(-1);
      lua.Pop(1);
      return b;
    }
    // 
    protected static void set_boolean(ILuaState lua, int idx, string key, bool b) {
      idx = LuaVM.NormalizeIndex(lua, idx);
      lua.PushBoolean(b);
      lua.SetField(idx, key);
    }
    // 
    protected static T get_member<T>(ILuaState lua, int idx, string key) {
      idx = LuaVM.NormalizeIndex(lua, idx);
      lua.GetField(idx, key);
      T obj = (T)lua.ToUserData(-1);
      lua.Pop(1);
      return obj;
    }
    // 
    protected static void set_member<T>(ILuaState lua, int idx, string key, T obj) {
      idx = LuaVM.NormalizeIndex(lua, idx);
      lua.PushLightUserData(obj);
      lua.SetField(idx, key);
    }
    //
    protected static string get_identifier(ILuaState lua, int idx) {
      return get_member<string>(lua, idx, IdentifierKey);
    }
    // 
    protected static void set_identifier(ILuaState lua, int idx, string identifier) {
      set_member<string>(lua, idx, IdentifierKey, identifier);
    }
    //
    [Conditional("UNITY_EDITOR")]
    protected static void check_identifier(ILuaState lua, int idx, string identifier) {
      if (!get_identifier(lua, idx).Contains(identifier)) {
        lua.L_Error("identifier({0}) is not match", identifier);
      }
    }
    //
    protected static T get_internal<T>(ILuaState lua, int idx) {
      return get_member<T>(lua, idx, InternalKey);
    }
    // 
    protected static void set_internal<T>(ILuaState lua, int idx, T obj) {
      set_member<T>(lua, idx, InternalKey, obj);
    }
    //
    protected static GameObject get_gameobject(ILuaState lua, int idx) {
      return get_member<GameObject>(lua, idx, GameObjectKey);
    }
    // 
    protected static void set_gameobject(ILuaState lua, int idx, GameObject obj) {
      set_member<GameObject>(lua, idx, GameObjectKey, obj);
    }
    //
    protected static T get_component<T>(ILuaState lua, int idx) where T : UnityEngine.Component {
      T c = get_internal<T>(lua, idx);
      if (c == null) {
        GameObject obj = get_gameobject(lua, idx);
        c = obj.GetComponent<T>();
      }
      return c;
    }
    //
    protected static void register_class(ILuaState lua, string klassname, NameFuncPair[] funcs) {
      int n = lua.GetTop();
      lua.NewTable();
      lua.PushValue(-1);
      lua.SetGlobal(klassname);
      set_identifier(lua, -1, klassname);
      lua.L_SetFuncs(funcs, 0);
      lua.SetTop(n);
    }
    protected static void register_class_derived(ILuaState lua, string klassname, NameFuncPair[] funcs, string superklassname) {
      int n = lua.GetTop();
      // -- describing object oriented class in Lua
      // Test = {}
      // function Test:new()
      //   o = {}
      //   setmetatable(o, self)
      //   self.__index = self
      //   return o
      // end
      // function Test:hoge()
      //   return 3
      // end
      // NewTest = Test:new()
      // function NewTest:hoge()
      //   return 4
      // end
      // ----------------------------
      // o = {}
      lua.NewTable();
      // setmetatable(o, self)
      lua.GetGlobal(superklassname); // self
      lua.SetMetaTable(-2); // -> o
      // self.__index = self
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      lua.PushValue(-1);
      lua.SetGlobal(klassname);
      set_identifier(lua, -1, klassname + superklassname);
      lua.L_SetFuncs(funcs, 0);
      lua.SetTop(n);
    }
    //
    protected static void extend_class(ILuaState lua, string klassname, NameFuncPair[] funcs) {
      int n = lua.GetTop();
      lua.GetGlobal(klassname);
      if (lua.IsNil(-1)) {
        throw new Exception(string.Format("unknown class {0}", klassname));
      }
      lua.L_SetFuncs(funcs, 0);
      lua.SetTop(n);
    }
    //
    protected static void add_constant_numbers(ILuaState lua, string klassname, NameNumberPair[] numbers) {
      int n = lua.GetTop();
      lua.GetGlobal(klassname);
      foreach (NameNumberPair nn in numbers) {
        lua.PushNumber(nn.number);
        lua.SetField(-2, nn.name);
      }
      lua.SetTop(n);
    }
    //
    protected static int push_new<T>(ILuaState lua, string klassname, T component, GameObject obj) {
      lua.NewTable();
      lua.GetGlobal(klassname);
      lua.SetMetaTable(-2);
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      set_internal<T>(lua, -1, component);
      set_gameobject(lua, -1, obj);
      return 1;
    }



    // スタック上の指定位置にあるテーブルの要素をコピーする
    protected static void copy_table(ILuaState lua, int dst, int src) {
      dst = LuaVM.NormalizeIndex(lua, dst);
      src = LuaVM.NormalizeIndex(lua, src);
      int sz = lua.RawLen(src);
      for (int i = 1; i <= sz; ++i) {
        lua.RawGetI(src, i);
        lua.RawSetI(dst, i);
      }
    }
    // スタック上の指定インデクスから指定個数の要素をテーブルにセットする
    protected static void set_value_to_table(ILuaState lua, int dst, int src, int nval) {
      dst = LuaVM.NormalizeIndex(lua, dst);
      src = LuaVM.NormalizeIndex(lua, src);
      for (int i = 1; i <= nval; ++i) {
        lua.PushValue(src + i - 1);
        lua.RawSetI(dst, i);
      }
    }
    protected static Vector2 get_vector2(ILuaState lua, int idx) {
      int n = lua.GetTop();
      Vector2 v = new Vector2();
      if (lua.IsTable(idx)) {
        lua.RawGetI(idx, 1);
        lua.RawGetI(idx, 2);
        v.x = (float)lua.ToNumber(-3);
        v.y = (float)lua.ToNumber(-2);
      } else {
        v.x = (float)lua.ToNumber(idx+0);
        v.y = (float)lua.ToNumber(idx+1);
      }
      lua.SetTop(n);
      return v;
    }
    protected static Vector3 get_vector3(ILuaState lua, int idx) {
      int n = lua.GetTop();
      Vector3 v = new Vector3();
      if (lua.IsTable(idx)) {
        lua.RawGetI(idx, 1);
        lua.RawGetI(idx, 2);
        lua.RawGetI(idx, 3);
        v.x = (float)lua.ToNumber(-3);
        v.y = (float)lua.ToNumber(-2);
        v.z = (float)lua.ToNumber(-1);
      } else {
        v.x = (float)lua.ToNumber(idx+0);
        v.y = (float)lua.ToNumber(idx+1);
        v.z = (float)lua.ToNumber(idx+2);
      }
      lua.SetTop(n);
      return v;
    }
  }
  public class LuaClassCommonUnityObject<U> : LuaClassCommon {
    public static System.Action<ILuaState, U> constructorHook;
  }
}
