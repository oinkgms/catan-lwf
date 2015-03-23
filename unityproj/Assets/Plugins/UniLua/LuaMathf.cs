using UnityEngine;
using System.Collections;
using UniLua;

namespace catan {
  public class LuaMathf {
    public static void LoadLibs(ILuaState lua) {
      Vec2.LoadLibs(lua);
      Vec3.LoadLibs(lua);
    }
    internal static int vec_new(ILuaState lua, int klass, int nval) {
      lua.NewTable();
      if (klass < 0) {
        lua.PushValue(klass-1);
      } else {
        lua.PushValue(klass);
      }
      lua.SetMetaTable(-2);
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      for (int i = 1; i <= nval; ++i) {
        lua.PushInteger(0);
        lua.RawSetI(-2, i);
      }
      return 1;
    }
    // 左側引数のメタテーブルをコピーします
    internal static int vec_new2(ILuaState lua, string klassname, int nval) {
      lua.NewTable();
      lua.GetMetaTable(1);
      lua.SetMetaTable(-2);
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      for (int i = 1; i <= nval; ++i) {
        lua.PushInteger(0);
        lua.RawSetI(-2, i);
      }
      return 1;
    }
    private static void copy_value(ILuaState lua, int nval, int dst, int src) {
      for (int i = 1; i <= nval; ++i) {
        lua.PushValue(src + i - 1);
        if (dst < 0) {
          lua.RawSetI(dst-1, i);
        } else {
          lua.RawSetI(dst, i);
        }
      }
    }
    private static void copy_array(ILuaState lua, int nval, int dst, int src) {
      int sz = Mathf.Min(nval, lua.RawLen(src));
      for (int i = 1; i <= sz; ++i) {
        lua.RawGetI(src, i);
        lua.RawSetI(dst, i);
      }
    }
    private static int vec_dump(ILuaState lua, int nval, int src) {
      string txt = string.Format("vec{0}(", nval);
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src, i);
        double a = lua.ToNumber(-1);
        txt += string.Format(" {0},", a);
        lua.Pop(1);
      }
      txt += ")";
      Debug.Log(txt);
      return 1;
    }
    private static int vec_add(ILuaState lua, int nval, int dst, int src0, int src1) {
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        double a = lua.ToNumber(-1);
        lua.RawGetI(src1, i);
        double b = lua.ToNumber(-1);
        lua.Pop(2);
        lua.PushNumber(a + b);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_sub(ILuaState lua, int nval, int dst, int src0, int src1) {
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        double a = lua.ToNumber(-1);
        lua.RawGetI(src1, i);
        double b = lua.ToNumber(-1);
        lua.Pop(2);
        lua.PushNumber(a - b);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_mul(ILuaState lua, int nval, int dst, int src0, int src1) {
      double b = lua.ToNumber(src1);
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        double a = lua.ToNumber(-1);
        lua.Pop(1);
        lua.PushNumber(a * b);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_div(ILuaState lua, int nval, int dst, int src0, int src1) {
      double b = 1.0 / lua.ToNumber(src1);
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        double a = lua.ToNumber(-1);
        lua.Pop(1);
        lua.PushNumber(a * b);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_magnitude(ILuaState lua, int nval, int src) {
      double r = 0.0;
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src, i);
        double a = lua.ToNumber(-1);
        lua.Pop(1);
        r += a * a;
      }
      lua.PushNumber(Mathf.Sqrt((float)r));
      return 1;
    }
    private static int vec_normalized(ILuaState lua, int nval, int dst, int src) {
      double r = 0.0;
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src, i);
        double a = lua.ToNumber(-1);
        lua.Pop(1);
        r += a * a;
      }
      r = 1.0 / (double)Mathf.Sqrt((float)r);
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src, i);
        double a = lua.ToNumber(-1);
        lua.Pop(1);
        lua.PushNumber(a * r);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_dot(ILuaState lua, int nval, int src0, int src1) {
      double r = 0.0;
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        double a = lua.ToNumber(-1);
        lua.RawGetI(src1, i);
        double b = lua.ToNumber(-1);
        lua.Pop(2);
        r += a * b;
      }
      lua.PushNumber(r);
      return 1;
    }
    private static int vec3_cross(ILuaState lua, int nval, int dst, int src0, int src1) {
      Vector3 va = new Vector3();
      Vector3 vb = new Vector3();
      for (int i = 1; i <= nval; ++i) {
        lua.RawGetI(src0, i);
        va[i-1] = (float)lua.ToNumber(-1);
        lua.RawGetI(src1, i);
        vb[i-1] = (float)lua.ToNumber(-1);
        lua.Pop(2);
      }
      Vector3 vc = Vector3.Cross(va, vb);
      for (int i = 1; i <= nval; ++i) {
        lua.PushNumber(vc[i-1]);
        lua.RawSetI(dst, i);
      }
      return 1;
    }
    private static int vec_member(ILuaState lua, int xyz) {
      int sz = lua.GetTop();
      if (sz > 1) {
        lua.PushValue(2);
        lua.PushValue(2);
        lua.RawSetI(1, xyz);
      } else {
        lua.RawGetI(1, xyz);
      }
      return 1;
    }
    //
    public class Vec2 {
      private static string klassName = "vec2";
      private const int elementMax = 2;
      public static void LoadLibs(ILuaState lua) {
        int n = lua.GetTop();
        lua.NewTable();
        lua.PushValue(-1);
        lua.SetGlobal(klassName);
        NameFuncPair[] mfuncs = new NameFuncPair[]{
          // static
          new NameFuncPair("dot", L_vec2_dot),
          //
          new NameFuncPair("new", L_vec2_new),
          new NameFuncPair("x", L_vec2_x),
          new NameFuncPair("y", L_vec2_y),
          new NameFuncPair("dump", L_vec2_dump),
          new NameFuncPair("__add", L_vec2_add),
          new NameFuncPair("__sub", L_vec2_sub),
          new NameFuncPair("__mul", L_vec2_mul),
          new NameFuncPair("__div", L_vec2_div),
          new NameFuncPair("magnitude", L_vec2_magnitude),
          new NameFuncPair("normalized", L_vec2_normalized),
          //new NameFuncPair("x", L_vec2_div),
          //new NameFuncPair("y", L_vec2_div),
        };
        lua.L_SetFuncs(mfuncs, 0);
        lua.SetTop(n);
      }
      public static void NewVec2(ILuaState lua, Vector2 vec) {
        lua.GetGlobal(klassName);
        LuaMathf.vec_new(lua, -1, elementMax);
        lua.PushNumber(vec.x);
        lua.PushNumber(vec.y);
        copy_value(lua, elementMax, -3, -2);
        lua.Pop(2);
      }
      // @class vec2
      // @desc 二つのベクトルにおける内積を求めます
      // @decl vec2 dot(v0, v1)
      // @param v0 ベクトル
      // @param v1 ベクトル
      // @result 内積値
      // @sample local v0 = vec2:new(0, 1)
      // @sample local v1 = vec2:new(1, 0)
      // @sample local dp = vec2.dot(v0, v1)
      private static int L_vec2_dot(ILuaState lua) {
        return LuaMathf.vec_dot(lua, elementMax, 1, 2);
      }
      // @desc ベクトルを生成します
      // @decl vec2 new(self)
      // @result インスタンス
      // @sample local v0 = vec2:new() -- (0, 0)で初期化
      // @sample local v1 = vec2:new(0, 1) -- (0, 1)で初期化
      // @sample local v2 = vec2:new({1, 2}) -- (1, 2)で初期化
      private static int L_vec2_new(ILuaState lua) {
        int narg = lua.GetTop();
        LuaMathf.vec_new(lua, 1, elementMax);
        if (narg == 2) {
          copy_array(lua, elementMax, 3, 2);
        } else
        if (narg == elementMax + 1) {
          copy_value(lua, elementMax, 4, 2);
        }
        return 1;
      }
      // @decl double x(self)
      private static int L_vec2_x(ILuaState lua) {
        return vec_member(lua, 1);
      }
      // @decl double y(self)
      private static int L_vec2_y(ILuaState lua) {
        return vec_member(lua, 2);
      }
      // @decl dump(self)
      private static int L_vec2_dump(ILuaState lua) {
        return LuaMathf.vec_dump(lua, elementMax, 1);
      }
      // @internal
      private static int L_vec2_add(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_add(lua, elementMax, 3, 1, 2);
        return 1;
      }
      // @internal
      private static int L_vec2_sub(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_sub(lua, elementMax, 3, 1, 2);
        return 1;
      }
      // @internal
      private static int L_vec2_mul(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_mul(lua, elementMax, 3, 1, 2);
        return 1;
      }
      // @internal
      private static int L_vec2_div(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_div(lua, elementMax, 3, 1, 2);
        return 1;
      }
      // @desc ベクトルの大きさを求める
      // @decl double magnitude(self)
      private static int L_vec2_magnitude(ILuaState lua) {
        return LuaMathf.vec_magnitude(lua, elementMax, 1);
      }
      // @desc 正規化したベクトルを返す
      // @decl vec2 normalized(self)
      private static int L_vec2_normalized(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_normalized(lua, elementMax, 2, 1);
        return 1;
      }
    }
    public class Vec3 {
      private static string klassName = "vec3";
      private const int elementMax = 3;
      public static void LoadLibs(ILuaState lua) {
        int n = lua.GetTop();
        lua.NewTable();
        lua.PushValue(-1);
        lua.SetGlobal(klassName);
        NameFuncPair[] mfuncs = new NameFuncPair[]{
          // static 
          new NameFuncPair("dot", L_vec3_dot),
          new NameFuncPair("cross", L_vec3_cross),
          //
          new NameFuncPair("new", L_vec3_new),
          new NameFuncPair("x", L_vec3_x),
          new NameFuncPair("y", L_vec3_y),
          new NameFuncPair("z", L_vec3_z),
          new NameFuncPair("dump", L_vec3_dump),
          new NameFuncPair("__add", L_vec3_add),
          new NameFuncPair("__sub", L_vec3_sub),
          new NameFuncPair("__mul", L_vec3_mul),
          new NameFuncPair("__div", L_vec3_div),
          new NameFuncPair("magnitude", L_vec3_magnitude),
          new NameFuncPair("normalized", L_vec3_normalized),
        };
        lua.L_SetFuncs(mfuncs, 0);
        lua.SetTop(n);
      }
      public static void NewVec3(ILuaState lua, Vector3 vec) {
        lua.GetGlobal(klassName);
        LuaMathf.vec_new(lua, -1, elementMax);
        lua.PushNumber(vec.x);
        lua.PushNumber(vec.y);
        lua.PushNumber(vec.z);
        copy_value(lua, elementMax, -4, -3);
        lua.Pop(3);
      }
      // @class vec3
      private static int L_vec3_dot(ILuaState lua) {
        return LuaMathf.vec_dot(lua, elementMax, 1, 2);
      }
      private static int L_vec3_cross(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        return LuaMathf.vec3_cross(lua, elementMax, 3, 1, 2);
      }
      private static int L_vec3_new(ILuaState lua) {
        int narg = lua.GetTop();
        LuaMathf.vec_new(lua, 1, elementMax);
        if (narg == 2) {
          copy_array(lua, elementMax, 3, 2);
        } else
        if (narg == elementMax + 1) {
          copy_value(lua, elementMax, 5, 2);
        }
        return 1;
      }
      private static int L_vec3_x(ILuaState lua) {
        return vec_member(lua, 1);
      }
      private static int L_vec3_y(ILuaState lua) {
        return vec_member(lua, 2);
      }
      private static int L_vec3_z(ILuaState lua) {
        return vec_member(lua, 3);
      }
      private static int L_vec3_dump(ILuaState lua) {
        return LuaMathf.vec_dump(lua, elementMax, 1);
      }
      private static int L_vec3_add(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_add(lua, elementMax, 3, 1, 2);
        return 1;
      }
      private static int L_vec3_sub(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_sub(lua, elementMax, 3, 1, 2);
        return 1;
      }
      private static int L_vec3_mul(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_mul(lua, elementMax, 3, 1, 2);
        return 1;
      }
      private static int L_vec3_div(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_div(lua, elementMax, 3, 1, 2);
        return 1;
      }
      private static int L_vec3_magnitude(ILuaState lua) {
        return LuaMathf.vec_magnitude(lua, elementMax, 1);
      }
      private static int L_vec3_normalized(ILuaState lua) {
        LuaMathf.vec_new2(lua, klassName, elementMax);
        LuaMathf.vec_normalized(lua, elementMax, 2, 1);
        return 1;
      }
    }







  }
}
