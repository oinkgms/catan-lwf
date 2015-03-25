using UnityEngine;
using System.Collections;

#if CATAN_ENABLE_UNILUA
using UniLua;
using catan.lwf;
namespace catan {
  namespace LuaBinding {
    public class ComponentSpriteObject : LuaClassCommon {
      internal static string klassName = "SpriteObject";
      public static void Register(ILuaState lua) {
        NameFuncPair[] mfuncs = new NameFuncPair[]{
          new NameFuncPair("set_color", L_set_color),
        };
        register_class(lua, klassName, mfuncs);
        LuaGameObject.constructorHook += add_lwf;
        Debug.Log("HOG2E");
      }
      private static void add_lwf(ILuaState lua, GameObject obj) {
        SpriteObject sobj = obj.GetComponent<SpriteObject>();
        Debug.Log("HOGE");
        push_new<SpriteObject>(lua, klassName, sobj, obj);
      }
      // @class MaskableGraphic
      // @desc 色を設定する
      // @decl set_color(self, color)
      // @param color color
      // @note colorクラス参照
      private static int L_set_color(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        return 0;
      }
    }
  }
}
#endif
