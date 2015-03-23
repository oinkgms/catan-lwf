using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniLua;

namespace catan {
  public class LuaComponentUI {
    public static void RegisterClass(ILuaState lua) {
      LuaComponentMaskableGraphic.RegisterClass(lua);
      LuaComponentUIText.RegisterClass(lua);
      LuaComponentUIImage.RegisterClass(lua);
      //LuaComponentUIButton.RegisterClass(lua);
    }
  }
  public class LuaComponentMaskableGraphic : LuaClassCommonUnityObject<MaskableGraphic> {
    internal static string klassName = "MaskableGraphic";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        new NameFuncPair("set_color", L_set_color),
      };
      register_class(lua, klassName, mfuncs);
    }
    // @class MaskableGraphic
    // @desc 色を設定する
    // @decl set_color(self, color)
    // @param color color
    // @note colorクラス参照
    private static int L_set_color(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      MaskableGraphic mg = get_component<MaskableGraphic>(lua, 1);
      Color col = ColorClass.ToColor(lua, 2);
      mg.color = col;
      return 0;
    }
  }







  public class LuaComponentUIText : LuaClassCommonUnityObject<Text> {
    private static string klassName = "UIText";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        new NameFuncPair("set_text", L_set_text),
        new NameFuncPair("get_font_size", L_get_font_size),
        new NameFuncPair("set_font_size", L_set_font_size),
        new NameFuncPair("set_resize_text_for_best_fit", L_set_resize_text_for_best_fit),
        new NameFuncPair("set_resize_text_min_size", L_set_resize_text_min_size),
        new NameFuncPair("set_resize_text_max_size", L_set_resize_text_max_size),
        new NameFuncPair("set_alignment", L_set_alignment),
      };
      register_class_derived(lua, klassName, mfuncs, LuaComponentMaskableGraphic.klassName);
      NameNumberPair[] mnumbers = new NameNumberPair[]{
        new NameNumberPair("alignment_upper_left", (double)TextAnchor.UpperLeft),
        new NameNumberPair("alignment_upper_center", (double)TextAnchor.UpperCenter),
        new NameNumberPair("alignment_upper_right", (double)TextAnchor.UpperRight),
        new NameNumberPair("alignment_middle_left", (double)TextAnchor.MiddleLeft),
        new NameNumberPair("alignment_middle_center", (double)TextAnchor.MiddleCenter),
        new NameNumberPair("alignment_middle_right", (double)TextAnchor.MiddleRight),
        new NameNumberPair("alignment_lower_left", (double)TextAnchor.LowerLeft),
        new NameNumberPair("alignment_lower_center", (double)TextAnchor.LowerCenter),
        new NameNumberPair("alignment_lower_right", (double)TextAnchor.LowerRight),
      };
      add_constant_numbers(lua, klassName, mnumbers);
    }
    public static void ExtendClass(ILuaState lua, NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
    //
    public static int PushNew(ILuaState lua, GameObject obj) {
      return push_new<Text>(lua, klassName, null, obj);
    }
    // @class UIText < MaskableGraphic
    // @desc テキストを設定する
    // @decl set_text(self, text)
    // @param text テキスト
    private static int L_set_text(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.text = lua.ToString(2);
      return 0;
    }
    // @desc フォントサイズ取得
    // @decl int get_font_size(self)
    // @result フォントサイズ
    private static int L_get_font_size(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      lua.PushInteger(uitext.fontSize);
      return 1;
    }
    // @desc フォントサイズ設定
    // @decl set_font_size(self, size)
    // @param size フォントサイズ
    private static int L_set_font_size(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.fontSize = lua.ToInteger(2);
      return 0;
    }
    // @desc resizeTextForBestFit
    // @decl set_resize_text_for_best_fit(self, flag)
    // @param flag Should the text be allowed to auto resized.
    private static int L_set_resize_text_for_best_fit(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.resizeTextForBestFit = lua.ToBoolean(2);
      return 0;
    }
    // @desc resizeTextMinSize
    // @decl set_resize_text_min_size(self, size)
    // @param size フォントサイズ
    private static int L_set_resize_text_min_size(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.resizeTextMinSize = lua.ToInteger(2);
      return 0;
    }
    // @desc resizeTextMaxSize
    // @decl set_resize_text_max_size(self, size)
    // @param size フォントサイズ
    private static int L_set_resize_text_max_size(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.resizeTextMaxSize = lua.ToInteger(2);
      return 0;
    }
    // @desc テキストのアライメント設定
    // @decl set_alignment(self, alignment)
    // @param alignment アライメント指定
    // @note UIText.alignment_upper_left
    // @note UIText.alignment_upper_center
    // @note UIText.alignment_upper_right
    // @note UIText.alignment_middle_left
    // @note UIText.alignment_middle_center
    // @note UIText.alignment_middle_right
    // @note UIText.alignment_lower_left
    // @note UIText.alignment_lower_center
    // @note UIText.alignment_lower_right
    private static int L_set_alignment(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Text uitext = get_component<Text>(lua, 1);
      uitext.alignment = (TextAnchor)lua.ToInteger(2);
      return 0;
    }
    //





  }
  public class LuaComponentUIImage : LuaClassCommonUnityObject<Image> {
    private static string klassName = "UIImage";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        new NameFuncPair("set_sprite", L_set_sprite),
      };
      register_class_derived(lua, klassName, mfuncs, LuaComponentMaskableGraphic.klassName);
    }
    public static void ExtendClass(ILuaState lua, NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
    //
    public static int PushNew(ILuaState lua, GameObject obj) {
      return push_new<Image>(lua, klassName, null, obj);
    }
    // @class UIImage < MaskableGraphic
    // @decl set_sprite(self, sprite)
    // @param sprite スプライト
    private static int L_set_sprite(ILuaState lua) {
      check_identifier(lua, 1, klassName);
      Image uiimage = get_component<Image>(lua, 1);
      uiimage.sprite = (Sprite)lua.ToUserData(2);
      return 0;
    }






  }
  //
#if false
  public class LuaComponentUIButton : LuaClassCommonUnityObject<Button> {
    private static string klassName = "UIButton";
    public static void RegisterClass(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        //new NameFuncPair("set_text", L_set_text),
      };
      register_class(lua, klassName, mfuncs);
      lua.GetGlobal(klassName);
      lua.PushNil();
      lua.SetField(-2, "on_pointer_down");
      lua.PushNil();
      lua.SetField(-2, "on_pointer_up");
      lua.Pop(1);
    }
    public static void ExtendClass(ILuaState lua, NameFuncPair[] funcs) {
      extend_class(lua, klassName, funcs);
    }
    //
    public static int PushNew(ILuaState lua, GameObject obj) {
      return push_new<Button>(lua, klassName, null, obj);
    }









    //private static int L_set_text(ILuaState lua) {
      //check_identifier(lua, 1, klassName);
      //Text uitext = get_component(lua, 1);
      //uitext.text = lua.ToString(2);
      return 0;
    }






  }
#endif
}
