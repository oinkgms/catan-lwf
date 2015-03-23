using UnityEngine;
using System.Collections;
using UniLua;

namespace catan {
  public class LuaComponentRectTransform : LuaClassCommonUnityObject<RectTransform> {
    private static string klassName = "RectTransform";
    private static string superKlassName = "Transform";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        //
        new NameFuncPair("get_size_delta", L_get_size_delta),
        new NameFuncPair("set_size_delta", L_set_size_delta),
        new NameFuncPair("get_pivot", L_get_pivot),
        new NameFuncPair("set_pivot", L_set_pivot),
        new NameFuncPair("get_anchored_position", L_get_anchored_position),
        new NameFuncPair("set_anchored_position", L_set_anchored_position),
        //new NameFuncPair("get_local_euler", L_get_local_euler),
        //new NameFuncPair("set_local_euler", L_set_local_euler),
        //new NameFuncPair("get_lossy_scale", L_get_lossy_scale),
        //new NameFuncPair("get_local_scale", L_get_local_scale),
        //new NameFuncPair("set_local_scale", L_set_local_scale),
        //new NameFuncPair("get_sibling_index", L_get_sibling_index),
        //new NameFuncPair("set_sibling_index", L_set_sibling_index),
        //new NameFuncPair("set_as_first_sibling", L_set_as_first_sibling),
        //new NameFuncPair("set_as_last_sibling", L_set_as_last_sibling),
        //new NameFuncPair("find", L_find),
      };
      register_class_derived(lua, klassName, mfuncs, superKlassName);
    }
    public static void ExtendClass(ILuaState lua, NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
    //
    public static int PushNew(ILuaState lua, RectTransform trs) {
      return push_new<RectTransform>(lua, klassName, trs, trs.gameObject);
    }
    // @class RectTransform < Transform
    // @class_desc `UnityEngine.RectTransform` のラッパー。
    // @class_desc `Transform` を継承しています。
    // @desc sizeDelta
    // @decl vec2 get_size_delta(self)
    // @result sizeDelta
    private static int L_get_size_delta(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector2 pos = get_internal<RectTransform>(lua, 1).sizeDelta;
      LuaMathf.Vec2.NewVec2(lua, pos);
      return 1;
    }
    // @desc sizeDelta
    // @decl set_size_delta(self, x, y)
    // @decl set_size_delta(self, v)
    // @param x X座標
    // @param y Y座標
    // @param v vec2(x, y)
    private static int L_set_size_delta(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<RectTransform>(lua, 1).sizeDelta = get_vector2(lua, 2);
      return 0;
    }
    // @desc pivot
    // @decl vec2 get_pivot(self)
    private static int L_get_pivot(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector2 v = get_internal<RectTransform>(lua, 1).pivot;
      LuaMathf.Vec2.NewVec2(lua, v);
      return 1;
    }
    // @desc pivot
    // @decl set_pivot(self, x, y)
    // @decl set_pivot(self, v)
    // @param x X座標
    // @param y Y座標
    // @param v vec2(x, y)
    private static int L_set_pivot(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<RectTransform>(lua, 1).pivot = get_vector2(lua, 2);
      return 0;
    }
    // @desc anchoredPosition
    // @decl vec2 get_anchored_position(self)
    private static int L_get_anchored_position(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector2 v = get_internal<RectTransform>(lua, 1).anchoredPosition;
      LuaMathf.Vec2.NewVec2(lua, v);
      return 1;
    }
    // @desc anchoredPosition
    // @decl set_anchored_position(self, x, y)
    // @decl set_anchored_position(self, v)
    // @param x X座標
    // @param y Y座標
    // @param v vec2(x, y)
    private static int L_set_anchored_position(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<RectTransform>(lua, 1).anchoredPosition = get_vector2(lua, 2);
      return 0;
    }
  }
  public class LuaComponentTransform : LuaClassCommonUnityObject<Transform> {
    private static string klassName = "Transform";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        //
        new NameFuncPair("get_position", L_get_position),
        new NameFuncPair("set_position", L_set_position),
        new NameFuncPair("get_local_position", L_get_local_position),
        new NameFuncPair("set_local_position", L_set_local_position),
        new NameFuncPair("get_euler", L_get_euler),
        new NameFuncPair("set_euler", L_set_euler),
        new NameFuncPair("get_local_euler", L_get_local_euler),
        new NameFuncPair("set_local_euler", L_set_local_euler),
        new NameFuncPair("get_lossy_scale", L_get_lossy_scale),
        new NameFuncPair("get_local_scale", L_get_local_scale),
        new NameFuncPair("set_local_scale", L_set_local_scale),
        new NameFuncPair("get_sibling_index", L_get_sibling_index),
        new NameFuncPair("set_sibling_index", L_set_sibling_index),
        new NameFuncPair("set_as_first_sibling", L_set_as_first_sibling),
        new NameFuncPair("set_as_last_sibling", L_set_as_last_sibling),
        new NameFuncPair("find", L_find),
      };
      register_class(lua, klassName, mfuncs);
    }
    public static void ExtendClass(ILuaState lua, NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
    //
    public static int PushNew(ILuaState lua, Transform trs) {
      return push_new<Transform>(lua, klassName, trs, trs.gameObject);
    }
    //
#if false
    private static Vector3 get_vector3(ILuaState lua, int idx) {
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
#endif
    // @class Transform
    // @desc positionを取得する
    // @decl vec3 get_position(self)
    // @result world position
    private static int L_get_position(ILuaState lua) {
      Vector3 pos = get_internal<Transform>(lua, 1).position;
      LuaMathf.Vec3.NewVec3(lua, pos);
      return 1;
    }
    // @desc positionを設定する
    // @decl set_position(self, x, y, z)
    // @decl set_position(self, pos)
    // @param x X座標
    // @param y Y座標
    // @param z Z座標
    // @param pos 座標を入れた配列
    // @sample g.transform:set_position(0, 0, 0)
    // @sample local t = { 1, 2, 3 }
    // @sample g.transform:set_position(t)
    private static int L_set_position(ILuaState lua) {
      get_internal<Transform>(lua, 1).position = get_vector3(lua, 2);
      return 0;
    }
    // @decl vec3 get_local_position(self)
    private static int L_get_local_position(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector3 pos = get_internal<Transform>(lua, 1).localPosition;
      LuaMathf.Vec3.NewVec3(lua, pos);
      return 1;
    }
    // @decl set_local_position(self, x, y, z)
    // @decl set_local_position(self, pos)
    // @param x X座標
    // @param y Y座標
    // @param z Z座標
    // @param pos 座標を入れた配列
    private static int L_set_local_position(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).localPosition = get_vector3(lua, 2);
      return 0;
    }
    // @decl vec3 get_euler(self)
    private static int L_get_euler(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Quaternion rot = get_internal<Transform>(lua, 1).rotation;
      LuaMathf.Vec3.NewVec3(lua, rot.eulerAngles);
      return 1;
    }
    // @decl set_euler(self, x, y, z)
    // @decl set_euler(self, pos)
    // @param x X座標
    // @param y Y座標
    // @param z Z座標
    // @param pos オイラー角を入れた配列
    private static int L_set_euler(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).rotation = Quaternion.Euler(get_vector3(lua, 2));
      return 0;
    }
    // @decl vec3 get_local_euler(self)
    private static int L_get_local_euler(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Quaternion rot = get_internal<Transform>(lua, 1).localRotation;
      LuaMathf.Vec3.NewVec3(lua, rot.eulerAngles);
      return 1;
    }
    // @decl set_local_euler(self, x, y, z)
    // @decl set_local_euler(self, pos)
    // @param x X座標
    // @param y Y座標
    // @param z Z座標
    // @param pos オイラー角を入れた配列
    private static int L_set_local_euler(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).localRotation = Quaternion.Euler(get_vector3(lua, 2));
      return 0;
    }
    // @decl vec3 get_lossy_scale(self)
    private static int L_get_lossy_scale(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector3 sca = get_internal<Transform>(lua, 1).lossyScale;
      LuaMathf.Vec3.NewVec3(lua, sca);
      return 1;
    }
    // @decl vec3 get_local_scale(self)
    private static int L_get_local_scale(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Vector3 sca = get_internal<Transform>(lua, 1).localScale;
      LuaMathf.Vec3.NewVec3(lua, sca);
      return 1;
    }
    // @decl set_local_scale(self, x, y, z)
    // @decl set_local_scale(self, pos)
    // @param x X軸の拡大率
    // @param y Y軸の拡大率
    // @param z Z軸の拡大率
    // @param pos 各軸の拡大率を入れた配列
    private static int L_set_local_scale(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).localScale = get_vector3(lua, 2);
      return 0;
    }

    // @desc sibling index
    // @decl int get_sibling_index(self)
    // @result インデックス
    private static int L_get_sibling_index(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      int idx = get_internal<Transform>(lua, 1).GetSiblingIndex();
      lua.PushInteger(idx);
      return 1;
    }
    // @desc sibling index
    // @decl set_sibling_index(self, index)
    // @param index 指定するインデックス
    private static int L_set_sibling_index(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).SetSiblingIndex(lua.ToInteger(-1));
      return 0;
    }
    // @desc sibling index
    // @decl set_as_first_sibling(self)
    private static int L_set_as_first_sibling(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).SetAsFirstSibling();
      return 0;
    }
    // @desc sibling index
    // @decl set_as_last_sibling(self)
    private static int L_set_as_last_sibling(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      get_internal<Transform>(lua, 1).SetAsLastSibling();
      return 0;
    }
    // @desc Transform.Find()
    // @decl GameObject find(self, name)
    // @param name 名前 or パス
    // @result 見つかったGameObject
    private static int L_find(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Transform trs = get_internal<Transform>(lua, 1);
      Transform child = trs.Find(lua.ToString(2));
      if (child != null) {
        LuaGameObject.PushNew(lua, child.gameObject);
      } else {
        lua.PushNil();
      }
      return 1;
    }

  }
}
