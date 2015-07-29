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
          new NameFuncPair("set_mult_color", L_set_mult_color),
          new NameFuncPair("set_add_color", L_set_add_color),
          new NameFuncPair("set_lerp_color", L_set_lerp_color),
          new NameFuncPair("clear_color_adjustment", L_clear_color_adjustment),
          new NameFuncPair("set_fps", L_set_fps),
          new NameFuncPair("set_play_speed", L_set_play_speed),
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
        //Debug.Log(so);
        //Debug.Log(movie);
        so.player.GotoAndPlayMovie("_root", movie);
        return 0;
      }
      // @desc 乗算色を指定する
      // @decl set_mult_color(self, color)
      // @param color カラー
      private static int L_set_mult_color(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        Color col = ColorClass.ToColor(lua, 2);
        so.player.multColor = col;
        return 0;
      }
      // @desc 加算色を指定する
      // @decl set_add_color(self, color)
      // @param color カラー
      private static int L_set_add_color(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        Color col = ColorClass.ToColor(lua, 2);
        so.player.addColor = col;
        return 0;
      }
      // @desc 補間色を指定する
      // @decl set_lerp_color(self, color)
      // @param color カラー
      private static int L_set_lerp_color(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        Color col = ColorClass.ToColor(lua, 2);
        so.player.lerpColor = col;
        return 0;
      }
      // @desc 色いじりをクリアする
      // @decl clear_color_adjustment(self)
      // @param color カラー
      private static int L_clear_color_adjustment(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        so.player.ClearColorAdjustment();
        return 0;
      }
      private static int L_set_fps(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        so.player.SetFrameRate(lua.ToInteger(2));
        return 0;
      }
      private static int L_set_play_speed(ILuaState lua) {
        check_identifier(lua, 1, klassName);
        SpriteObject so = get_component<SpriteObject>(lua, 1);
        so.player.SetPlaySpeed((float)lua.ToNumber(2));
        return 0;
      }
    }
  }
}
#endif
