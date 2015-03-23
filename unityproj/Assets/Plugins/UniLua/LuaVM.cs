using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UniLua;

namespace catan {
  namespace LuaBinding {
    public class LuaHoge {
      public static void Register(ILuaState lua) {
        Debug.Log(lua);
      }
    }
  }
  public class LuaVM : MonoBehaviour {
    public delegate void LowFreqCallback(ILuaState lua);
    private static LuaVM _instance = null;
    private LuaDebugMenu _debugMenu = null;
    private ILuaState _luaState = null;
    private List<int> _exportFunctions = new List<int>();
    private List<LowFreqCallback> _lfc = new List<LowFreqCallback>();
    private UtilClass _utilClass = new UtilClass();
    private static bool _androidPlatform;
    private static string _waitFunc = @"
function wait(sec)
  local n = 0
  while sec > n do
      coroutine.yield(0)
      n = n + delta_time()
  end
end
";
    private static bool _scriptCache = false;
    //
    public static LuaVM instance {
      get {
        if (_instance == null) {
          GameObject obj = new GameObject("catan.LuaVM");
          obj.AddComponent<LuaVM>(); // call awake
        }
        return _instance;
      }
    }
    public LuaDebugMenu debugMenu {
      get {
        return _debugMenu;
      }
    }
    public ILuaState luaState {
      get {
        return _luaState;
      }
    }
    public static bool useScriptCache {
      get { return _scriptCache; }
      set { _scriptCache = value; }
    }
    static string path_hook(string filename) {
      string r;
      if (_androidPlatform) {
        WWW www = new WWW(Path.Combine(Path.Combine(Application.streamingAssetsPath, "LuaRoot"), filename));
        string temppath = Path.Combine(Application.temporaryCachePath, "LuaRoot");
        r = Path.Combine(temppath, filename);
        Directory.CreateDirectory(Path.GetDirectoryName(r));
        while (!www.isDone) {}
        try {
          File.WriteAllBytes(r, www.bytes);
        }
        catch (System.Exception e) {
          LuaDebugMenu.LogTab.Add(e.ToString());
        }
      } else {
        r = Path.Combine(Path.Combine(Application.streamingAssetsPath, "LuaRoot"), filename);
      }
      return r;
    }
    void create_state() {
      if (_luaState == null) {
        LuaFile.SetPathHook(path_hook);
        _luaState = LuaAPI.NewState();
        _luaState.L_OpenLibs();
        _luaState.L_DoString(_waitFunc);
        register_default_functions();
        register_common_libs();
        register_libs();
      }
    }
    void destroy_state() {
      _luaState = null;
    }
    //
    void Awake() {
      if (_instance != null) {
        Destroy(this);
      } else {
        _instance = this;
        _androidPlatform = Application.streamingAssetsPath.Contains("://");
        create_state();
        StartCoroutine("gc");
        StartCoroutine("low_freq_callback");
#if UNITY_EDITOR
        {
#else
        if (Debug.isDebugBuild) {
#endif
          _debugMenu = LuaDebugMenu.instance;
        }
      }
    }
    //
    void OnDestroy() {
      destroy_state();
      if (_instance == this) {
        _instance = null;
      }
    }
    // 
    void Start() {
    }
    // 
    void Update() {
      _utilClass.Update();
    }
    //
    private IEnumerator gc() {
      while (true) {
        //System.GC.Collect();
        yield return new WaitForSeconds(1.0f);
      }
    }
    //
    private IEnumerator low_freq_callback() {
      while (true) {
        foreach (LowFreqCallback lfc in _lfc) {
          lfc(_luaState);
          yield return null;
        }
        yield return new WaitForSeconds(1.0f);
      }
    }
    //
    public LuaThread CreateThread(LuaScriptBehaviour behaviour) {
      LuaThread th = new LuaThread(_luaState, behaviour);
      return th;
    }
    //
    public void AddLowFreqCallback(LowFreqCallback lfc) {
      _lfc.Add(lfc);
    }
    //
    public ArrayList DoString(string script) {
      int ini = _luaState.GetTop();
      _luaState.PushCSharpFunction(Traceback);
      int tracefunc = _luaState.GetTop();
      var status = _luaState.L_LoadString(script);
#if UNITY_EDITOR
      if (status != ThreadStatus.LUA_OK)
        throw new System.Exception("LOAD ERROR:\n" + script);
#endif
      _luaState.PCall(0, LuaDef.LUA_MULTRET, tracefunc);
      _luaState.Remove(tracefunc);
      int nr = _luaState.GetTop() - ini;
      var r = PackLuaValue(_luaState, ini, nr);
      _luaState.SetTop(ini);
      return r;
    }
    // スタックの値をArrayListにぶっこんで返す
    public static ArrayList PackLuaValue(ILuaState lua, int sidx, int num) {
      ArrayList result = new ArrayList();
      for (int i = 1; i <= num; ++i) {
        int idx = sidx + i;
        if (lua.IsNil(idx)) {
          result.Add(null);
        } else
        if (lua.Type(idx) == LuaType.LUA_TBOOLEAN) {
          result.Add(lua.ToBoolean(idx));
        } else
        if (lua.Type(idx) == LuaType.LUA_TNUMBER) {
          result.Add(lua.ToNumber(idx));
        } else
        if (lua.IsString(idx)) {
          result.Add(lua.ToString(idx));
        } else {
#if UNITY_EDITOR
          throw new System.Exception("UNKNOWN TYPE");
#endif
        }
      }
      return result;
    }
    //
    public void RegisterFunctions(UniLua.NameFuncPair[] namefuncs) {
      int n = _luaState.GetTop();
      _luaState.PushGlobalTable();
      _luaState.PushGlobalTable();
      _luaState.SetField(-2, "_G");
      _luaState.L_SetFuncs(namefuncs, 0);
      _luaState.SetTop(n);
    }
    public void RegisterFunction(UniLua.NameFuncPair namefunc) {
      NameFuncPair[] namefuncs = new NameFuncPair[]{ namefunc };
      RegisterFunctions(namefuncs);
    }
    private void register_default_functions() {
      NameFuncPair[] defines = new NameFuncPair[]{
        new NameFuncPair("delta_time", L_delta_time),
        new NameFuncPair("print", B_Print),
        new NameFuncPair("export_function", L_export_function),
        new NameFuncPair("unexport_function", L_unexport_function),
        new NameFuncPair("invoke_function", L_invoke_function),
      };
      RegisterFunctions(defines);
    }
    private void register_common_libs() {
      LuaThread.LoadLibs(_luaState);
      LuaSystemLib.Load(_luaState); // <-- @todo この書式に統一したい
      LuaGameObject.RegisterClass(_luaState);
      LuaComponentBehaviour.RegisterClass(_luaState);
      LuaComponentTransform.RegisterClass(_luaState);
      LuaComponentRectTransform.RegisterClass(_luaState);
      LuaComponentUI.RegisterClass(_luaState);
      LuaMathf.LoadLibs(_luaState);
      LuaUtilLib.Load(_luaState);
    }
    private void register_libs() {
      Assembly[] asms = System.AppDomain.CurrentDomain.GetAssemblies();
      System.Type[] args = new System.Type[]{ typeof(ILuaState) };
      System.Object[] objs = new System.Object[]{ _luaState };
      foreach (Assembly asm in asms) {
        System.Type[] types = asm.GetTypes();
        foreach (System.Type t in types) {
          if (t.IsClass && t.IsPublic && t.Namespace != null && t.Namespace.Contains("catan.LuaBinding")) {
            //Debug.Log(string.Format("{0} {1}", t.FullName, t.Namespace));
            MethodInfo mi = t.GetMethod("Register", args);
            if (mi != null) {
              //Debug.Log(mi.Name);
              mi.Invoke(null, objs);
            }
          }
        }
      }
    }
    //
    private static int L_delta_time(ILuaState lua) {
      lua.PushNumber(Time.deltaTime);
      return 1;
    }

    public static int B_Print( ILuaState lua ) {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      int n = lua.GetTop();
      lua.GetGlobal( "tostring" );
      for( int i=1; i<=n; ++i )
      {
        lua.PushValue( -1 );
        lua.PushValue( i );
        lua.Call( 1, 1 );
        string s = lua.ToString( -1 );
        if( s == null )
          return lua.L_Error("'tostring' must return a string to 'print'");
        if( i > 1 )
          sb.Append( "\t" );
        sb.Append( s );
        lua.Pop( 1 );
      }
      Debug.Log( sb.ToString() );
      return 0;
    }



    private int L_NewMetaTable(string tname) {
      _luaState.PushString(tname);
      _luaState.GetMetaTable(-1);  /* try to get metatable */
      if (!_luaState.IsNil(-1))  /* name already in use? */
        return 0;  /* leave previous value on top, but return 0 */
      _luaState.Pop(1);
      _luaState.NewTable();  /* create metatable */
      _luaState.PushValue(-1);
      _luaState.SetField(LuaDef.LUA_REGISTRYINDEX, tname);  /* registry.name = metatable */
      return 1;
    }

    // 関数をエクスポートします
    // luaスタックトップに関数を積んでから呼んでください
    // どのLuaStateに積んでいるか分からないので、引数で指定します
    // @result 関数ID
    public int ExportFunction(ILuaState lua) {
      int fid = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
      _exportFunctions.Add(fid);
      return fid;
    }
    // エクスポートした関数を解除します
    // @param fid 関数ID
    public void UnexportFunction(ILuaState lua, int fid) {
      lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, fid);
      _exportFunctions.RemoveAll(i => i == fid);
    }
    // エクスポートした関数を起動します
    // @param fid 関数ID
    // @param narg 引数の数（先にこの数分スタックに積んでおく必要がある
    // @param nresult 返値の数（予定されている数
    // @result 実際に返ってきた返値の数（スタックにこの分積まれている
    public int InvokeFunction(ILuaState lua, int fid, int narg, int nresult) {
      int n = lua.GetTop() - narg;
      lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, fid);
      if (narg > 0) {
        lua.Insert(n+1);
      }
      lua.Call(narg, nresult);
      if (lua.Status != ThreadStatus.LUA_OK) {
        Debug.LogError(lua.ToString(-1));
      }
      return lua.GetTop() - n;
    }
    // @desc スクリプトから呼べる関数を登録します
    // @param function 関数
    // @result 関数ID
    private static int L_export_function(ILuaState lua) {
      LuaVM vm = LuaVM.instance;
      int fid = vm.ExportFunction(lua);
      lua.PushInteger(fid);
      return 1;
    }
    // @desc 関数登録を解除します
    // @param fid 関数ID
    private static int L_unexport_function(ILuaState lua) {
      LuaVM vm = LuaVM.instance;
      int fid = lua.ToInteger(-1);
      vm.UnexportFunction(lua, fid);
      return 0;
    }
    // @desc 登録した関数を起動します
    // @param fid 関数ID
    private static int L_invoke_function(ILuaState lua) {
      LuaVM vm = LuaVM.instance;
      int sz = lua.GetTop();
      int narg = sz - 1;
      int fid = lua.ToInteger(1);
      for (int i = 0; i < narg; ++i) {
        lua.PushValue(2+i);
      }
      int nresult = vm.InvokeFunction(lua, fid, narg, LuaDef.LUA_MULTRET);
      return nresult;
    }

    // ケツからのインデクスを先頭からの絶対インデクスに変換する
    public static int NormalizeIndex(ILuaState lua, int idx) {
      if (idx < 0) {
        return Mathf.Max(lua.GetTop() + 1 + idx, 1);
      }
      return idx;
    }
    public static int Traceback(ILuaState lua) {
      var msg = lua.ToString(1);
      if(msg != null) {
        lua.L_Traceback(lua, msg, 1);
      }
      // is there an error object?
      else if(!lua.IsNoneOrNil(1)) {
        // try its `tostring' metamethod
        if(!lua.L_CallMeta(1, "__tostring")) {
          lua.PushString("(no error message)");
        }
      }
      return 1;
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void StackDump(ILuaState lua) {
      int sz = lua.GetTop();
      string txt = "";
      for (int i = 1; i <= sz; ++i) {
        txt += string.Format("{0}: {1} {2}\n", i, lua.TypeName(lua.Type(i)), stack_dump_value(lua, i));
      }
      Debug.Log(txt);
    }
    private static string stack_dump_value(ILuaState lua, int idx) {
#if UNITY_EDITOR
      LuaType t = lua.Type(idx);
      switch (t) {
      case LuaType.LUA_TNIL:
        return "nil";
      case LuaType.LUA_TBOOLEAN:
        return string.Format("{0}", lua.ToBoolean(idx));
      case LuaType.LUA_TUINT64:
        return string.Format("{0}", lua.ToUInt64(idx));
      case LuaType.LUA_TNUMBER:
        return string.Format("{0}", lua.ToNumber(idx));
      default:
        return "--";
      }
#else
      return "";
#endif
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Assert(ILuaState lua, bool cond) {
      if (!cond) {
        lua.L_Error("ERROR");
      }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(object msg) {
      Debug.Log(msg);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(object msg, Object ctxt) {
      Debug.Log(msg, ctxt);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError(object msg) {
      Debug.LogError(msg);
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError(object msg, Object ctxt) {
      Debug.LogError(msg, ctxt);
    }
  }
}
