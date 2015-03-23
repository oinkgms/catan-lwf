using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniLua;

namespace catan {
  public class LuaGameObject : LuaClassCommonUnityObject<GameObject> {
    private static string klassName = "GameObject";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("blank", L_blank),
        new NameFuncPair("current", L_current),
        new NameFuncPair("find", L_find),
        new NameFuncPair("load", L_load),
        new NameFuncPair("instantiate", L_instantiate),
        new NameFuncPair("destroy", L_destroy),
        //
        new NameFuncPair("set_active", L_set_active),
        new NameFuncPair("get_name", L_get_name),
        new NameFuncPair("set_name", L_set_name),
        new NameFuncPair("get_parent", L_get_parent),
        new NameFuncPair("set_parent", L_set_parent),
        new NameFuncPair("kick", L_kick),
        //
        new NameFuncPair("e_load_texture", L_load_texture),
      };
      register_class(lua, klassName, mfuncs);
    }
    public static void ExtendClass(ILuaState lua, UniLua.NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
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
    public static int PushNew(ILuaState lua, GameObject obj) {
      LuaVM.Assert(lua, obj != null);
      LuaGameObjectCache cache = obj.GetComponent<LuaGameObjectCache>();
      if (cache != null && cache.index == LuaConstants.LUA_NOREF) {
        Object.Destroy(cache);
        cache = null;
      }
      if (cache == null) {
        cache = obj.AddComponent<LuaGameObjectCache>();
        // o = {}
        lua.NewTable();
        // setmetatable(o, self)
        //lua.PushValue(-2); // self
        lua.GetGlobal(klassName);
        lua.SetMetaTable(-2); // -> o
        // self.__index = self
        lua.GetMetaTable(-1);
        lua.SetField(-1, "__index");
        // return o
        set_internal<GameObject>(lua, -1, obj);
        set_boolean(lua, -1, "is_prefab", false);
        //
        LuaScriptBehaviour lsb = obj.GetComponent<LuaScriptBehaviour>();
        if (lsb != null) {
          LuaComponentBehaviour.PushNew(lua, lsb);
          lua.SetField(-2, "behaviour");
        }
        LuaComponentTransform.PushNew(lua, obj.transform);
        lua.SetField(-2, "transform");
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null) {
          LuaComponentRectTransform.PushNew(lua, rt);
          lua.SetField(-2, "rect_transform");
        }
        LuaComponentUIText.PushNew(lua, obj);
        lua.SetField(-2, "ui_text");
        LuaComponentUIImage.PushNew(lua, obj);
        lua.SetField(-2, "ui_image");
        if (constructorHook != null) constructorHook(lua, obj);
        lua.PushValue(-1);
        int refidx = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
        cache.index = refidx;
        cache.luaState = lua;
      }
      lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, cache.index);
      //Debug.Log(string.Format("Find {0} {1}", objid, obj.name));
      if (get_internal<GameObject>(lua, -1) == null) {
        set_internal<GameObject>(lua, -1, obj);
      }
      return 1;
    }
    public static int PushNewAsPrefab(ILuaState lua, GameObject obj) {
      LuaVM.Assert(lua, obj != null);
      // o = {}
      lua.NewTable();
      // setmetatable(o, self)
      //lua.PushValue(-2); // self
      lua.GetGlobal(klassName);
      lua.SetMetaTable(-2); // -> o
      // self.__index = self
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      // return o
      set_internal<GameObject>(lua, -1, obj);
      set_boolean(lua, -1, "is_prefab", true);
      return 1;
    }
    //private static void push_component<T>(ILuaState lua, string name, GameObject obj) where T : LuaClassCommon {
    //    T.PushNew(lua, obj);
    //    lua.SetField(-2, name);
    //}
    // @internal
    // @class GameObject
    // @desc 継承用
    private static int L_blank(ILuaState lua) {
      PushNew(lua, null);
      return 1;
    }
    // @desc このスクリプトが動いているMonoBehaviourが貼り付いているGameObjectを取得する
    // @decl GameObject current()
    // @result GameObject
    // @sample local g = GameObject.current()
    private static int L_current(ILuaState lua) {
      LuaScriptBehaviour lsb = LuaScriptBehaviour.FindFromState(lua);
      GameObject obj = lsb.gameObject;
      PushNew(lua, obj);
      return 1;
    }
    // @desc 名前で探す
    // @decl GameObject find(name)
    // @param name 名前
    // @result GameObject
    // @sample local g = GameObject.find("HOGE")
    private static int L_find(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TSTRING);
      string name = lua.ToString(1);
      GameObject obj = GameObject.Find(name);
      if (obj != null) {
        PushNew(lua, obj);
      } else {
        lua.PushNil();
      }
      return 1;
    }
    // @desc リソースから生成する
    // @decl GameObject load(resname)
    // @param resname リソース名
    // @result GameObject
    // @sample local g = GameObject.load("hoge")
    // @note 内部でResources.Load<GameObject>(resname)を呼んでいます
    private static int L_load(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TSTRING);
      string name = lua.ToString(1);
      GameObject src = Resources.Load<GameObject>(name);
      if (src != null) {
        GameObject obj = (GameObject)GameObject.Instantiate(src);
        PushNew(lua, obj);
      } else {
        lua.PushNil();
      }
      return 1;
    }
    // @desc GameObjectを実体化する
    // @decl GameObject instantiate(object)
    // @param object GameObject
    // @result GameObject
    // @sample local g = GameObject.instantiate(Resources.load_game_object("hoge"))
    private static int L_instantiate(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject src = get_internal<GameObject>(lua, 1);
      GameObject obj = (GameObject)GameObject.Instantiate(src);
      Debug.Log(obj);
      LuaGameObjectCache cache = obj.GetComponent<LuaGameObjectCache>();
      //if (cache != null) Object.Destroy(cache);
      PushNew(lua, obj);
      return 1;
    }
    // @desc デストロる
    // @decl destroy(obj)
    // @param obj GameObject
    // @sample GameObject.destroy(o)
    private static int L_destroy(ILuaState lua) {
      GameObject obj = get_internal<GameObject>(lua, 1);
      GameObject.Destroy(obj);
      set_internal<GameObject>(lua, 1, null); // 一応参照を外しておく
      return 0;
    }
    // @desc 有効か無効か設定する
    // @decl set_active(self, flag)
    // @param flag on/off
    // @sample g:set_active(true)
    // @sample g:set_active(false)
    private static int L_set_active(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      bool f = lua.ToBoolean(2);
      obj.SetActive(f);
      return 0;
    }
    // @desc 名前を取得する
    // @decl string get_name(self)
    // @result 名前
    // @sample local name = g:get_name()
    private static int L_get_name(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      lua.PushString(obj.name);
      return 1;
    }
    // @desc 名前を設定する
    // @decl set_name(self, name)
    // @param name 名前
    // @sample g:set_name("NEW NAME!")
    private static int L_set_name(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      string name = lua.ToString(2);
      obj.name = name;
      return 0;
    }
    // @desc 親を取得する
    // @decl GameObject get_parent(self)
    // @result 親GameObject
    // @sample local parent = g:get_parent()
    private static int L_get_parent(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      if (obj.transform.parent != null) {
        //PushNew(lua, obj.transform.parent.gameObject);
        PushNew(lua, obj.transform.parent.gameObject);
      } else {
        lua.PushNil();
      }
      return 1;
    }
    // @desc 親を設定する
    // @decl set_parent(self, parent)
    // @param parent GameObject
    // @sample g:set_parent(parent)
    // @sample g:set_parent(nil)
    private static int L_set_parent(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      if (lua.IsNil(2)) {
        obj.transform.SetParent(null, false);
      } else {
        GameObject parent = get_internal<GameObject>(lua, 2);
        obj.transform.SetParent(parent.transform, false);
      }
      return 0;
    }
    // @desc 指定したGameObjectに貼り付いているLuaScriptの関数を呼び出す
    // @decl [result] kick(self, funcname[, args])
    // @param funcname 呼び出したい関数名
    // @param args 渡したい引数（いくつでも）
    // @result 呼び出した関数による
    // @sample g:kick("func")
    // @sample g:kick("some_args", 0, 1, 2)
    // @sample local r = g:kick("result")
    // @sample local a, b = g:kick("multi_results")
    private static int L_kick(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      string name = lua.ToString(2);
      int narg = lua.GetTop() - 2;
      lua.NewThread();
      ILuaState sublua = lua.ToThread(-1);
      lua.Pop(1);
      LuaScriptBehaviour lsb = obj.GetComponent<LuaScriptBehaviour>();
      lsb.PushSelfTable(sublua);
      for (int i = 0; i < narg; ++i) {
        lua.PushValue(3+i);
      }
      if (narg > 0) lua.XMove(sublua, narg);
      int nresult = lsb.InvokeScriptFunction(sublua, name, narg);
      sublua.XMove(lua, nresult);
      sublua.SetTop(0);
      return nresult;
    }


    // @internal
    private static int L_load_texture(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      GameObject obj = get_internal<GameObject>(lua, 1);
      string name = lua.ToString(2);
      Image image = obj.GetComponent<Image>();
      Sprite sprite = Resources.Load<Sprite>(name);
      image.sprite = sprite;
      return 0;
    }





  }
}
