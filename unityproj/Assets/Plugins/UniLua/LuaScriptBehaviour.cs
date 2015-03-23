using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniLua;

namespace catan {
  public class LuaScriptBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    //
    private class LuaScript {
      internal enum CachedFunction {
        AWAKE,
        START,
        UPDATE,
        LATE_UPDATE,
        FIXED_UPDATE,
        COROUTINE,
        MAX,
      };
      private static string[] _cachedFunctionNames = new string[]{
        "awake",
        "start",
        "update",
        "late_update",
        "fixed_update",
        "coroutine",
      };
      private ILuaState _lua;
      private string _filename;
      private int _retainCount = 0;
      private Dictionary<string, int> _functions = new Dictionary<string, int>();
      private int[] _cachedFunctions = new int[(int)CachedFunction.MAX];
      private int _exportTableIndex;
      //
      internal LuaScript(ILuaState lua, string filename) {
        _lua = lua;
        _filename = filename;
        load();
      }
      private void load() {
        ThreadStatus status;
        LuaDebugMenu debugmenu = LuaDebugMenu.instance;
        if (Debug.isDebugBuild && debugmenu != null && debugmenu.connected) {
          status = _lua.L_DoString(debugmenu.LoadFile(_filename));
        } else {
          status = _lua.L_DoFile(_filename);
        }
        if (status != ThreadStatus.LUA_OK) {
          throw new Exception(_lua.ToString(-1));
        }
        if (!_lua.IsTable(-1)) {
          throw new Exception("framework main's return value is not a table");
        }
        _lua.PushValue(-1);
        _exportTableIndex = _lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
        store_functions();
        _lua.Pop(1);
      }
      private void unload() {
        _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _exportTableIndex);
      }
      internal LuaScript Retain() {
        ++_retainCount;
        return this;
      }
      internal int Release() {
        int c =  --_retainCount;
        if (c == 0) {
          unstore_functions();
          unload();
        }
        return c;
      }
      private void store_functions() {
        _functions.Clear();
        AppendFunctions();
      }
      private void unstore_functions() {
        foreach (string k in _functions.Keys) {
          _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _functions[k]);
        }
        _functions.Clear();
        for (int i = 0; i < (int)CachedFunction.MAX; ++i) {
          _cachedFunctions[i] = LuaConstants.LUA_NOREF;
        }
      }
      internal void AppendFunctions() {
        _lua.PushNil();
        while (_lua.Next(-2)) {
          if (_lua.IsString(-2) && _lua.IsFunction(-1)) {
            string name = _lua.ToString(-2);
            if (HasFunction(name)) {
              _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _functions[name]);
            }
            int reg = _lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            _functions[name] = reg;
          } else {
            _lua.Pop(1); // L_Refで一つ消費される
          }
        }
        for (int i = 0; i < (int)CachedFunction.MAX; ++i) {
          string name = _cachedFunctionNames[i];
          _cachedFunctions[i] = (_functions.ContainsKey(name))? _functions[name] : LuaConstants.LUA_NOREF;
        }
      }
      internal bool HasFunction(CachedFunction type) {
        return _cachedFunctions[(int)type] != LuaConstants.LUA_NOREF;
      }
      internal bool HasFunction(string funcname) {
        return _functions.ContainsKey(funcname);
      }
      internal int InvokeFunction(ILuaState lua, CachedFunction type, int narg, int selftable=LuaConstants.LUA_NOREF) {
        int func = _cachedFunctions[(int)type];
        if (func != LuaConstants.LUA_NOREF) {
          lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, func);
          if (narg > 0) lua.Insert(-(narg+1));
          return invoke_function(lua, narg, selftable);
        }
        return 0;
      }
      internal int InvokeFunction(ILuaState lua, string funcname, int narg, int selftable=LuaConstants.LUA_NOREF) {
        LuaVM.Assert(lua, _functions.ContainsKey(funcname));
        int key = _functions[funcname];
        lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, key);
        int b = LuaVM.NormalizeIndex(lua, -(narg+1));
        lua.Insert(b);
        int nresult = invoke_function(lua, narg, selftable);
        return nresult;
      }
      internal int invoke_function(ILuaState lua, int narg, int selftable=LuaConstants.LUA_NOREF) {
        int top = lua.GetTop() - (narg + 1);
        // insert `traceback' function
        lua.PushCSharpFunction(Traceback);
        int b = LuaVM.NormalizeIndex(lua, -(narg+2));
        lua.Insert(b);
        var status = lua.PCall(narg, LuaDef.LUA_MULTRET, b);
        if (status != ThreadStatus.LUA_OK) {
          Debug.LogError(status);
          Debug.LogError(lua.ToString(-1));
        }
        // remove `traceback' function
        lua.Remove(b);
        return lua.GetTop() - top;
      }
      internal bool PushFunction(ILuaState lua, CachedFunction type) {
        int func = _cachedFunctions[(int)type];
        if (func < 0) { return false; }
        lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, func);
        return true;
      }
      internal static int Traceback(ILuaState lua) {
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
      internal void PushExportTable(ILuaState lua) {
        lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, _exportTableIndex);
      }
    }
    // ----------------------------------------------------
    private static LuaVM _vm;
    private static Dictionary<string, LuaScript> _cachedScripts = new Dictionary<string, LuaScript>();
    private static Dictionary<ILuaState, LuaScriptBehaviour> _behaviours = new Dictionary<ILuaState, LuaScriptBehaviour>();
    //
    private List<LuaThread> _threads = new List<LuaThread>();
    private LuaThread _mainThread = null;
    private ILuaState _lua;
    private LuaThread _currentThread = null;
    private bool _errorOccured = false;
    private LuaScript _script;
    private List<int> _anonymousFunctions = new List<int>();
    public string _luaScriptFile = "";
    private bool _loaded = false;
    private int _selfTableIndex;
    //
    public string luaScriptFile {
      get { return _luaScriptFile; }
      set {
        if (!_loaded) {
          load_script_file(value);
        }
      }
    }
    public static LuaScriptBehaviour FindFromState(ILuaState lua) {
      return _behaviours[lua];
    }
    //
    private static LuaVM retain_vm() {
      if (_vm == null) {
        _vm = LuaVM.instance;
        if (_vm == null) {
          GameObject obj = new GameObject("catan.LuaVM");
          _vm = obj.AddComponent<LuaVM>();
        }
      }
      return _vm;
    }
    //
    void Awake() {
      _mainThread = retain_vm().CreateThread(this);
      _lua = _mainThread.luaState;
      _behaviours[_lua] = this;
      //if (!_mainThread.luaState.CheckStack(1000)) {
      //  Debug.LogError("CANT ALLOC STACK");
      //}
      if (!string.IsNullOrEmpty(_luaScriptFile)) {
        load_script_file(_luaScriptFile);
      }
      invoke_script_function_cached(LuaScript.CachedFunction.AWAKE);
    }
    //
    void OnDestroy() {
      if (_loaded) InvokeScriptFunction(_lua, "on_destroy", 0);
      unexport_anonymous_functions();
      unload_script_file();
      _behaviours.Remove(_lua);
    }
    // 動的にAddComponentした場合はこっちで呼び出す
    private void load_script_file(string filename) {
      _luaScriptFile = filename;
      if (LuaVM.useScriptCache) {
        if (_cachedScripts.ContainsKey(filename)) {
          _script = _cachedScripts[filename].Retain();
        }
      }
      if (_script == null) {
        _script = new LuaScript(_lua, _luaScriptFile).Retain();
        if (LuaVM.useScriptCache) {
          _cachedScripts[_luaScriptFile] = _script;
        }
      }
      _loaded = true;
      //
      _lua.NewTable();
      _script.PushExportTable(_lua);
      _lua.SetMetaTable(-2);
      _lua.GetMetaTable(-1);
      _lua.SetField(-1, "__index");
      _selfTableIndex = _lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
    }
    private void unload_script_file() {
      if (_script != null) {
        _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, _selfTableIndex);
        if (_script.Release() == 0) {
          if (LuaVM.useScriptCache && _cachedScripts.ContainsKey(_luaScriptFile)) {
            _cachedScripts.Remove(_luaScriptFile);
          }
        }
      }
    }
    public void AppendFromSelfTable() {
      if (_script != null) {
        PushSelfTable(_lua);
        _script.AppendFunctions();
        _lua.Pop(1);
      }
    }
    public void PushSelfTable(ILuaState lua) {
      lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, _selfTableIndex);
    }
    // 無名関数をエクスポートします
    // luaスタックトップに関数を積んでから呼んでください
    // @result 関数ID
    public int ExportAnonymousFunction(ILuaState lua) {
      int fid = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
      _anonymousFunctions.Add(fid);
      return fid;
    }
    // エクスポートした無名関数を解除します
    // @param fid 関数ID
    public void UnexportAnonymousFunction(int fid) {
      _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, fid);
      _anonymousFunctions.RemoveAll(i => i == fid);
    }
    // エクスポートした全ての関数を解除します
    private void unexport_anonymous_functions() {
      foreach (int fid in _anonymousFunctions) {
        _lua.L_Unref(LuaDef.LUA_REGISTRYINDEX, fid);
      }
    }
    // エクスポートした無名関数を起動します
    // @param fid 関数ID
    // @param narg 引数の数（先にこの数分スタックに積んでおく必要がある
    // @param nresult 返値の数（予定されている数
    // @result 実際に返ってきた返値の数（スタックにこの分積まれている
    public int InvokeAnonymousFunction(ILuaState lua, int fid, int narg, int nresult) {
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
    // AssetBundle testx.unity3d is built for StandaloneWindows
#if false //UNITY_STANDALONE
    IEnumerator Start() {
      invoke_script_function_cached(LuaScript.CachedFunction.START);
      invoke_script_thread(LuaScript.CachedFunction.COROUTINE);

      // -- sample code for loading binary Asset Bundles --------------------
      String s = "file:///"+Application.streamingAssetsPath+"/testx.unity3d";
      WWW www = new WWW(s);
      yield return www;
      if(www.assetBundle.mainAsset != null) {
        TextAsset cc = (TextAsset)www.assetBundle.mainAsset;
        var status = _lua.L_LoadBytes(cc.bytes, "test");
        if( status != ThreadStatus.LUA_OK )
        {
          throw new Exception( _lua.ToString(-1) );
        }
        status = _lua.PCall( 0, 0, 0);
        if( status != ThreadStatus.LUA_OK )
        {
          throw new Exception( _lua.ToString(-1) );
        }
        Debug.Log("---- call done ----");
      }
    }
#else
    void Start() {
      invoke_script_function_cached(LuaScript.CachedFunction.START);
      invoke_script_thread(LuaScript.CachedFunction.COROUTINE);
    }
#endif
    void Update() {
      invoke_script_function_cached(LuaScript.CachedFunction.UPDATE);
      update_lua_threads();
    }
    void LateUpdate() {
      invoke_script_function_cached(LuaScript.CachedFunction.LATE_UPDATE);
    }
    void FixedUpdate() {
      invoke_script_function_cached(LuaScript.CachedFunction.FIXED_UPDATE);
    }
    private void update_lua_threads() {
      if (_errorOccured) return;
      foreach (LuaThread t in _threads) {
        if (t.isRunning) {
          _currentThread = t;
          t.Resume();
          if (t.isError) {
            _errorOccured = true;
          }
          _currentThread = null;
        }
      }
      _threads.RemoveAll((t) => t.isExit);
    }
    private void invoke_script_function_cached(LuaScript.CachedFunction type) {
      if (_script != null && _script.HasFunction(type)) {
        PushSelfTable(_lua);
        _script.InvokeFunction(_lua, type, 1);
      }
    }
    public int InvokeScriptFunction(ILuaState lua, string funcname, int narg) {
      if (_script != null && _script.HasFunction(funcname)) {
        PushSelfTable(lua);
        if (narg > 0) lua.Insert(LuaVM.NormalizeIndex(lua, -(narg+1)));
        return _script.InvokeFunction(lua, funcname, narg+1);
      }
      return 0;
    }
    private void invoke_script_thread(LuaScript.CachedFunction type) {
      if (_script != null && _script.PushFunction(_lua, type)) {
        StartThread(_lua);
      }
    }
    // 渡された関数で新規スレッドを起動する
    public LuaThread StartThread(ILuaState lua) {
      LuaThread th = _vm.CreateThread(this);
      _threads.Add(th);
      ILuaState newlua = th.luaState;
      lua.XMove(newlua, 1);
      th.Start(0);
      return th;
    }
    private void invoke_handler(string funcname) {
      int n = _lua.GetTop();
      InvokeScriptFunction(_lua, funcname, 0);
      _lua.SetTop(n);
    }
    public void OnClick() {
      invoke_handler("on_click");
    }
    public void OnPointerDown(PointerEventData eventdata) {
      invoke_handler("on_pointer_down");
    }
    public void OnPointerUp(PointerEventData eventdata) {
      invoke_handler("on_pointer_up");
    }
  }
}
