using UnityEngine;
using System.Collections;

#if CATAN_LWF_ENABLE_UNILUA
using UniLua;
using catan.lwf;
namespace catan {
  namespace LuaBinding {
    public class ComponentSpriteObject : LuaClassCommon {
      internal static string klassName = "SpriteObject";
      public static void Register(ILuaState lua) {
        NameFuncPair[] mfuncs = new NameFuncPair[]{
          new NameFuncPair("play", L_play),
        };
        register_class(lua, klassName, mfuncs);
        LuaGameObject.constructorHook += add_sprite_object;
      }
      private static void add_sprite_object(ILuaState lua, GameObject obj) {
        SpriteObject sobj = obj.GetComponent<SpriteObject>();
        push_new<SpriteObject>(lua, klassName, sobj, obj);
        lua.SetField(-2, "sprite_object");
      }
      // @class SpriteObject
      // @desc アニメを再生する
      // @decl play(self, animtag)
      // @param animtag タグ
      private static int L_play(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        string movie = lua.ToString(2);
        //Debug.Log(movie);
        so.player.GotoAndPlayMovie("_root", movie);
        return 0;
      }
    }
  }
}
#endif
