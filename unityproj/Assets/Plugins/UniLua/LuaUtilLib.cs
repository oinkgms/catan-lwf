using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

namespace catan {
  public class LuaUtilLib {
    public static void Load(ILuaState lua) {
      StringClass.Load(lua);
      TableClass.Load(lua);
      OSClass.Load(lua);
      ColorClass.Load(lua);
      ResourcesClass.Load(lua);
      ApplicationClass.Load(lua);
      PlayerPrefsClass.Load(lua);
      UtilClass.Load(lua);
    }
  }
  public class StringClass : LuaClassCommon {
    private static string klassName = "string";
    public static void Load(ILuaState lua) {
      NameFuncPair[] funcs = new NameFuncPair[]{
        new NameFuncPair("format2", L_string_format2),
      };
      extend_class(lua, klassName, funcs);
    }
    private static int L_string_format2(ILuaState lua) {
      int n = lua.GetTop();
      string fmt = lua.ToString(1);
      int narg = n - 1;
      string txt = "";
      switch (narg) {
      case 1: txt = string.Format(fmt, get_as_object(lua, 2)); break;
      case 2: txt = string.Format(fmt, get_as_object(lua, 2), get_as_object(lua, 3)); break;
      }
      lua.PushString(txt);
      return 1;
    }
    private static System.Object get_as_object(ILuaState lua, int idx) {
      if (lua.Type(idx) == LuaType.LUA_TNUMBER) {
        //return new int[]{ (int)lua.ToNumber(idx) };
        return (int)lua.ToNumber(idx);
      }
      return lua.ToObject(idx);
    }
  }
  public class OSClass : LuaClassCommon {
    private static string klassName = "os";
    public static void Load(ILuaState lua) {
      lua.GetGlobal(klassName);
      if (!lua.IsNil(-1)) {
        NameFuncPair[] funcs = new NameFuncPair[]{
          new NameFuncPair("time_since_startup", L_time_since_startup),
          new NameFuncPair("time_since_level_load", L_time_since_level_load),
        };
        lua.L_SetFuncs(funcs, 0);
      }
      lua.Pop(1);
    }
    // @class os
    // @desc os.time_since_startup
    // @result アプリが実行されてからの経過時間
    // @note Time.realtimeSinceStartup
    private static int L_time_since_startup(ILuaState lua) {
      lua.PushNumber(Time.realtimeSinceStartup);
      return 1;
    }
    // @desc os.time_since_level_load
    // @result レベルロードからの経過時間
    // @note Time.timeSinceLevelLoad
    private static int L_time_since_level_load(ILuaState lua) {
      lua.PushNumber(Time.timeSinceLevelLoad);
      return 1;
    }
  }
  public class TableClass : LuaClassCommon {
    private static string klassName = "table";
    public static void Load(ILuaState lua) {
      lua.GetGlobal(klassName);
      if (!lua.IsNil(-1)) {
        NameFuncPair[] funcs = new NameFuncPair[]{
          new NameFuncPair("map", L_table_map),
          new NameFuncPair("map_", L_table_map_),
          new NameFuncPair("unite", L_table_unite),
        };
        lua.L_SetFuncs(funcs, 0);
      }
      lua.Pop(1);
    }
    // @class table
    // @class_desc `table` を拡張したクラスです。
    // @desc table.map
    // @decl table map(t, f)
    // @param t テーブル
    // @param f 関数
    // @result 処理後のテーブル
    private static int L_table_map(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TTABLE);
      lua.L_CheckType(2, LuaType.LUA_TFUNCTION);
      int n = lua.RawLen(1);
      lua.CreateTable(n, 0);
      for (int i = 1; i <= n; ++i) {
        lua.PushValue(2);
        lua.RawGetI(1, i);
        lua.Call(1, 1);
        lua.RawSetI(3, i);
      }
      return 1;
    }
    // @desc table.map!
    // @decl table map_(t, f)
    // @param t テーブル
    // @param f 関数
    // @note この関数はtを破壊します
    private static int L_table_map_(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TTABLE);
      lua.L_CheckType(2, LuaType.LUA_TFUNCTION);
      int n = lua.RawLen(1);
      for (int i = 1; i <= n; ++i) {
        lua.PushValue(2);
        lua.RawGetI(1, i);
        lua.Call(1, 1);
        lua.RawSetI(1, i);
      }
      return 0;
    }
    // @desc テーブルを連結します
    // @decl table unite(t0, t1)
    // @param t0 テーブル
    // @param t1 テーブル
    // @result t0とt1を連結したテーブルを返す
    private static int L_table_unite(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TTABLE);
      lua.L_CheckType(2, LuaType.LUA_TTABLE);
      lua.NewTable();
      int idx = 1;
      for (int i = 1; i <= 2; ++i) {
        int n = lua.RawLen(i);
        for (int j = 1; j <= n; ++j) {
          lua.RawGetI(i, j);
          lua.RawSetI(-2, idx++);
        }
      }
      return 1;
    }
  }
  public class ColorClass : LuaClassCommon {
    private static string klassName = "color";
    //
    public static void Load(ILuaState lua) {
      define_prototype(lua);
    }
    private static void define_prototype(ILuaState lua) {
      int n = lua.GetTop();
      lua.NewTable();
      lua.PushValue(-1);
      lua.SetGlobal(klassName);
      for (int i = 1; i <= 4; ++i) {
        lua.PushNumber(0.0);
        lua.RawSetI(-2, i);
      }
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        // instance
        new NameFuncPair("new", L_color_new),
        new NameFuncPair("dump", L_color_dump),
        new NameFuncPair("r", L_color_r),
        new NameFuncPair("g", L_color_g),
        new NameFuncPair("b", L_color_b),
        new NameFuncPair("a", L_color_a),
        new NameFuncPair("__add", L_color_add),
        new NameFuncPair("__sub", L_color_sub),
        
      };
      lua.L_SetFuncs(mfuncs, 0);
      lua.SetTop(n);
    }
    public static int PushColor(ILuaState lua, int prototypestacked=0) {
      lua.NewTable();
      if (prototypestacked == 0) {
        lua.GetGlobal(klassName);
      } else
      if (prototypestacked < 0) {
        lua.PushValue(prototypestacked-1);
      } else {
        lua.PushValue(prototypestacked);
      }
      lua.SetMetaTable(-2);
      lua.GetMetaTable(-1);
      lua.SetField(-1, "__index");
      for (int i = 1; i <= 4; ++i) {
        lua.PushInteger(0);
        lua.RawSetI(-2, i);
      }
      return 1;
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    internal static void check_value(ILuaState lua, int idx) {
      if (lua.RawLen(idx) != 4) {
        lua.L_Error("ERROR");
      }
    }
    public static Color ToColor(ILuaState lua, int idx) {
      idx = LuaVM.NormalizeIndex(lua, idx);
      check_value(lua, idx);
      int n = lua.GetTop();
      lua.RawGetI(idx, 1);
      float r = (float)lua.ToNumber(-1);
      lua.RawGetI(idx, 2);
      float g = (float)lua.ToNumber(-1);
      lua.RawGetI(idx, 3);
      float b = (float)lua.ToNumber(-1);
      lua.RawGetI(idx, 4);
      float a = (float)lua.ToNumber(-1);
      lua.SetTop(n);
      return new Color(r, g, b, a);
    }
    private static int access_member(ILuaState lua, int midx) {
      if (lua.GetTop() == 1) {
        lua.RawGetI(-1, midx);
      } else
      if (lua.GetTop() == 2) {
        double v = lua.L_CheckNumber(-1);
        lua.PushNumber(v);
        lua.RawSetI(-3, midx);
        lua.PushValue(-2);
      }
      return 1;
    }
    // @class color
    // @desc インスタンスを生成します
    // @decl color new(self)
    // @decl color new(self, r, g, b, a)
    // @decl color new(self, col)
    // @param r 赤要素
    // @param g 緑要素
    // @param b 青要素
    // @param a アルファ
    private static int L_color_new(ILuaState lua) {
      int narg = lua.GetTop() - 1;
      PushColor(lua);
      if (narg == 1 && lua.IsTable(-2)) {
        copy_table(lua, -1, -2);
      } else
      if (narg == 4) {
        set_value_to_table(lua, -1, 2, 4);
      }
      return 1;
    }
    // @desc 内容をダンプします
    // @decl dump(self)
    private static int L_color_dump(ILuaState lua) {
      string txt = "color(";
      for (int i = 1; i <= 4; ++i) {
        lua.RawGetI(-1, i);
        double a = lua.ToNumber(-1);
        txt += string.Format(" {0},", a);
        lua.Pop(1);
      }
      txt += ")";
      Debug.Log(txt);
      return 1;
    }
    // @desc 赤要素へアクセスします
    // @param self インスタンス
    // @note 引数なしで呼び出すと要素の値を返し、引数ありで呼び出すとその要素に値を設定します
    // @sample local col = color:new()
    // @sample print(col:r()) -- output 0
    // @sample col:r(1)
    // @sample print(col:r()) -- output 1
    private static int L_color_r(ILuaState lua) {
      return access_member(lua, 1);
    }
    // @desc 緑要素へアクセスします
    // @param self インスタンス
    // @note 引数なしで呼び出すと要素の値を返し、引数ありで呼び出すとその要素に値を設定します
    private static int L_color_g(ILuaState lua) {
      return access_member(lua, 2);
    }
    // @desc 青要素へアクセスします
    // @param self インスタンス
    // @note 引数なしで呼び出すと要素の値を返し、引数ありで呼び出すとその要素に値を設定します
    private static int L_color_b(ILuaState lua) {
      return access_member(lua, 3);
    }
    // @desc 不透明度要素へアクセスします
    // @param self インスタンス
    // @note 引数なしで呼び出すと要素の値を返し、引数ありで呼び出すとその要素に値を設定します
    private static int L_color_a(ILuaState lua) {
      return access_member(lua, 4);
    }
    // @desc 加算
    // @param a 左の値
    // @param b 右の値
    // @result 結果
    // @sample local a = color:new(1, 1, 0, 1)
    // @sample local b = color:new({1, 0, 0, 0})
    // @sample local c = a + b
    private static int L_color_add(ILuaState lua) {
      return color_expression(lua, (a, b) => a + b);
    }
    private static int L_color_sub(ILuaState lua) {
      return color_expression(lua, (a, b) => a - b);
    }
    private static int color_expression(ILuaState lua, System.Func<double, double, double> exp) {
      PushColor(lua, 1); // 左の値に準ずる
      for (int i = 1; i <= 4; ++i) {
        lua.RawGetI(1, i);
        double a = lua.L_CheckNumber(-1);
        lua.RawGetI(2, i);
        double b = lua.L_CheckNumber(-1);
        lua.Pop(2);
        lua.PushNumber(exp(a, b));
        lua.RawSetI(-2, i);
      }
      return 1;
    }

  }
  public class ResourcesClass : LuaClassCommon {
    private static string klassName = "Resources";
    //
    public static void Load(ILuaState lua) {
      int n = lua.GetTop();
      lua.NewTable();
      lua.PushValue(-1);
      lua.SetGlobal(klassName);
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("load_game_object", L_load_game_object),
        new NameFuncPair("load_sprite", L_load_sprite),
        // instance
      };
      lua.L_SetFuncs(mfuncs, 0);
      lua.SetTop(n);
    }
    // @class Resources
    // @desc GameObjectをロードします
    // @decl GameObject load_game_object(prefabname)
    // @param prefabname プレハブのパス
    // @return GameObject
    // @note これで取得した GameObject は transform 等を編集することは出来ません。
    // @note また GameObject.is_prefab が真になっています。
    // @sample local g = Resources.load_game_object("HOGE")
    // @sample print(g.is_prefab) -- <-- true
    // @sample local newg = GameObject.instantiate(g)
    // @sample print(newg.is_prefab) -- <-- false
    // @see GameObject.instantiate
    private static int L_load_game_object(ILuaState lua) {
      lua.L_CheckType(1, LuaType.LUA_TSTRING);
      GameObject obj = Resources.Load<GameObject>(lua.ToString(1));
      if (obj == null) {
        lua.PushNil();
      } else {
        LuaGameObject.PushNewAsPrefab(lua, obj);
      }
      return 1;
    }
    // @desc スプライトをロードします
    // @decl load_sprite(spritename)
    // @decl load_sprite(atlusname, spritename)
    // @return スプライト
    private static int L_load_sprite(ILuaState lua) {
      int n = lua.GetTop();
      for (int i = 1; i <= n; ++i) {
        lua.L_CheckType(i, LuaType.LUA_TSTRING);
      }
      if (n == 1) {
        Sprite sprite = Resources.Load<Sprite>(lua.ToString(1));
        lua.PushLightUserData(sprite);
      } else
      if (n == 2) {
        string atlus = lua.ToString(1);
        Sprite[] sprites = Resources.LoadAll<Sprite>(atlus);
        string name = lua.ToString(2);
        Sprite sprite = System.Array.Find<Sprite>(sprites, (spr) => spr.name.Equals(name));
#if UNITY_EDITOR
        if (sprite == null) {
          lua.L_Error("NOT FOUND {0} in {1}", name, atlus);
        }
#endif
        lua.PushLightUserData(sprite);
      }
      return 1;
    }
  }
  public class ApplicationClass : LuaClassCommon {
    private static string klassName = "Application";
    //
    public static void Load(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("load_level", L_load_level),
        new NameFuncPair("get_loaded_level_name", L_get_loaded_level_name),
        new NameFuncPair("is_editor", L_is_editor),
        new NameFuncPair("is_debug_build", L_is_development_build),
        new NameFuncPair("is_development_build", L_is_development_build),
        new NameFuncPair("target_frame_rate", L_target_frame_rate),
        // instance
      };
      register_class(lua, klassName, mfuncs);
    }
    // @class Application
    // @desc シーンをロードします
    // @decl load_level(index)
    // @decl load_level(name)
    // @param index シーン番号
    // @param name シーン名
    private static int L_load_level(ILuaState lua) {
      if (lua.IsString(1)) {
        string name = lua.ToString(1);
        LuaVM.Log(string.Format("LOAD LEVEL {0}", name));
        Application.LoadLevel(name);
      } else {
        int idx = lua.ToInteger(1);
        LuaVM.Log(string.Format("LOAD LEVEL {0}", idx));
        Application.LoadLevel(idx);
      }
      return 0;
    }
    // @desc 現在読み込んでいるシーンの名前を取得します
    // @decl string get_loaded_level_name()
    private static int L_get_loaded_level_name(ILuaState lua) {
      string name = Application.loadedLevelName;
      lua.PushString(name);
      return 1;
    }
    // @desc エディタ上で動作しているかどうか返す
    // @note Application.isEditor
    // @decl bool is_editor()
    private static int L_is_editor(ILuaState lua) {
      lua.PushBoolean(Application.isEditor);
      return 1;
    }
    // @desc development buildかどうか返す
    // @note Debug.isDebugBuild
    // @decl bool is_development_build()
    // @decl bool is_debug_build()
    private static int L_is_development_build(ILuaState lua) {
      lua.PushBoolean(Debug.isDebugBuild);
      return 1;
    }
    // @desc フレームレートの設定と取得
    // @note Application.targetFrameRate
    // @decl target_frame_rate(rate)
    // @decl int target_frame_rate()
    // @param rate フレームレート
    private static int L_target_frame_rate(ILuaState lua) {
      int n = lua.GetTop();
      if (n == 0) {
        lua.PushInteger(Application.targetFrameRate);
        return 1;
      } else {
        int rate = lua.ToInteger(-1);
        Application.targetFrameRate = rate;
        return 0;
      }
    }
  }
  public class PlayerPrefsClass : LuaClassCommon {
    private static string klassName = "PlayerPrefs";
    private static PlayerPrefsBase _implement = new PlayerPrefsBase();
    //
    public static void Load(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("delete_all", L_delete_all),
        new NameFuncPair("delete_key", L_delete_key),
        new NameFuncPair("get_float", L_get_float),
        new NameFuncPair("get_int", L_get_int),
        new NameFuncPair("get_string", L_get_string),
        new NameFuncPair("has_key", L_has_key),
        new NameFuncPair("save", L_save),
        new NameFuncPair("set_float", L_set_float),
        new NameFuncPair("set_int", L_set_int),
        new NameFuncPair("set_string", L_set_string),
        // instance
      };
      register_class(lua, klassName, mfuncs);
    }
    // カスタマイズしたい時はこれを継承する
    public class PlayerPrefsBase {
      public void DeleteAll() { PlayerPrefs.DeleteAll(); }
      public void DeleteKey(string key) { PlayerPrefs.DeleteKey(key); }
      public float GetFloat(string key, float defaultvalue=0.0f) { return PlayerPrefs.GetFloat(key, defaultvalue); }
      public int GetInt(string key, int defaultvalue=0) { return PlayerPrefs.GetInt(key, defaultvalue); }
      public string GetString(string key, string defaultvalue="") { return PlayerPrefs.GetString(key, defaultvalue); }
      public bool HasKey(string key) { return PlayerPrefs.HasKey(key); }
      public void Save() { PlayerPrefs.Save(); }
      public void SetFloat(string key, float v) { PlayerPrefs.SetFloat(key, v); }
      public void SetInt(string key, int v) { PlayerPrefs.SetInt(key, v); }
      public void SetString(string key, string v) { PlayerPrefs.SetString(key, v); }
    }
    public static PlayerPrefsBase implement {
      set { _implement = value; }
    }
    // @class PlayerPrefs
    // @desc DeleteAll
    // @decl delete_all()
    private static int L_delete_all(ILuaState lua) {
      _implement.DeleteAll();
      return 0;
    }
    // @desc DeleteKey
    // @decl delete_key(key)
    // @param key キー
    private static int L_delete_key(ILuaState lua) {
      string key = lua.ToString(1);
      _implement.DeleteKey(key);
      return 0;
    }
    // @desc GetFloat
    // @decl float get_float(key, default=0.0)
    // @param key キー
    // @param default キーが存在しない場合の値
    private static int L_get_float(ILuaState lua) {
      string key = lua.ToString(1);
      float val = (lua.GetTop() == 2)? (float)lua.ToNumber(2) : 0.0f;
      val = _implement.GetFloat(key, val);
      lua.PushNumber((double)val);
      return 1;
    }
    // @desc GetInt
    // @decl int get_int(key, defualt=0)
    // @param key キー
    // @param default キーが存在しない場合の値
    private static int L_get_int(ILuaState lua) {
      string key = lua.ToString(1);
      int val = (lua.GetTop() == 2)? lua.ToInteger(2) : 0;
      val = _implement.GetInt(key, val);
      lua.PushInteger(val);
      return 1;
    }
    // @desc GetString
    // @decl string get_string(key, defualt="")
    // @param key キー
    // @param default キーが存在しない場合の値
    private static int L_get_string(ILuaState lua) {
      string key = lua.ToString(1);
      string val = (lua.GetTop() == 2)? lua.ToString(2) : "";
      val = _implement.GetString(key, val);
      lua.PushString(val);
      return 1;
    }
    // @desc HasKey
    // @decl bool has_key(key)
    // @param key キー
    private static int L_has_key(ILuaState lua) {
      string key = lua.ToString(1);
      lua.PushBoolean(_implement.HasKey(key));
      return 1;
    }
    // @desc Save
    // @decl save()
    private static int L_save(ILuaState lua) {
      _implement.Save();
      return 0;
    }
    // @desc SetFloat
    // @decl set_float(key, value)
    // @param key キー
    // @param value 設定したい値
    private static int L_set_float(ILuaState lua) {
      string key = lua.ToString(1);
      double val = lua.ToNumber(2);
      _implement.SetFloat(key, (float)val);
      return 0;
    }
    // @desc SetInt
    // @decl set_int(key, value)
    // @param key キー
    // @param value 設定したい値
    private static int L_set_int(ILuaState lua) {
      string key = lua.ToString(1);
      int val = lua.ToInteger(2);
      _implement.SetInt(key, val);
      return 0;
    }
    // @desc SetString
    // @decl set_string(key, value)
    // @param key キー
    // @param value 設定したい値
    private static int L_set_string(ILuaState lua) {
      string key = lua.ToString(1);
      string val = lua.ToString(2);
      _implement.SetString(key, val);
      return 0;
    }
  }
  public class UtilClass : LuaClassCommon {
    private static UtilClass _instance;
    private static string klassName = "util";
    //
    public static void Load(ILuaState lua) {
      NameFuncPair[] mfuncs = new NameFuncPair[]{
        // static
        new NameFuncPair("timeout", L_timeout),
        // instance
      };
      register_class(lua, klassName, mfuncs);
    }
    private class Element {
      public int _fid;
      public float _time;
      public ILuaState _lua;
    }
    private List<Element> _timeouts = new List<Element>();
    public UtilClass() {
      _instance = this;
    }
    public void Update() {
      for (int i = 0; i < _timeouts.Count; ++i) {
        Element e = _timeouts[i];
        e._time -= Time.deltaTime;
        if (e._time <= 0.0f) {
          LuaScriptBehaviour lsb = LuaScriptBehaviour.FindFromState(e._lua);
          lsb.InvokeAnonymousFunction(e._lua, e._fid, 0, 0);
        }
      }
      _timeouts.RemoveAll(e => e._time <= 0.0f);
    }
    // @class util
    // @obsolete
    // @desc timeout
    // @decl (time, func)
    // @param time 遅延させる時間(秒)
    // @param func 関数
    // @sample util.timeout(5, function() print("5sec") end)
    private static int L_timeout(ILuaState lua) {
      double time = lua.ToNumber(1);
      LuaScriptBehaviour lsb = LuaScriptBehaviour.FindFromState(lua);
      Element e = new Element();
      e._fid = lsb.ExportAnonymousFunction(lua);
      e._time = (float)time;
      e._lua = lua;
      _instance._timeouts.Add(e);
      return 0;
    }
  }
}

