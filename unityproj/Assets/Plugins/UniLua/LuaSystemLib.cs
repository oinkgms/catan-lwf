using UnityEngine;
using System.Collections;
using UniLua;

namespace catan {
  public class LuaSystemLib {
    private static string klassName = "system";
    //
    public static void Load(ILuaState lua) {
      int n = lua.GetTop();
      lua.NewTable();
      lua.PushValue(-1);
      lua.SetGlobal(klassName);
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("dostring", L_dostring),
      };
      lua.L_SetFuncs(mfuncs, 0);
      lua.SetTop(n);
    }



    // @desc 与えられた文字列を実行します
    // @param str Luaスクリプト文字列
    // @result スクリプトの返値（あれば
    private static int L_dostring(ILuaState lua) {
      int n = lua.GetTop();
      string s = lua.ToString(-1);
      LuaVM.instance.DoString(s);
      return lua.GetTop() - n;
    }

  }
}
