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
          new NameFuncPair("load", L_load),
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
      // @desc GameObjectにSpriteObjectを貼り付けて読み込む
      // @decl load(gameobject, path, is_ui)
      // @param gameobject 対象となるGameObject
      // @param path LWFのファイルパス
      // @param is_ui レンダラがUI対応版かどうか
      private static int L_load(ILuaState lua) {
        GameObject obj = get_internal<GameObject>(lua, 1);
        string path = lua.ToString(2);
        bool is_ui = lua.ToBoolean(3);

        SpriteObject sobj = obj.AddComponent<SpriteObject>();
        sobj.lwfName = path;
        if (is_ui) sobj.renderType = SpriteObject.RenderType.UI;

        push_new<SpriteObject>(lua, klassName, sobj, obj);
        lua.SetField(1, "sprite_object");

        sobj.CreatePlayer();

        return 0;
      }
      // @desc アニメを再生する
      // @decl play(self, animtag)
      // @param animtag タグ
      private static int L_play(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        string movie = lua.ToString(2);
        Debug.Log(so);
        Debug.Log(movie);
        so.player.GotoAndPlayMovie("_root", movie);
        return 0;
      }
    }
  }
}
#endif
