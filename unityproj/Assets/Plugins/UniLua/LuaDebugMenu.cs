using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace catan {
  //import UnityEngine
  //import System.IO
  //import System.Runtime.Serialization
  //import System.Runtime.Serialization.Formatters.Binary

  //namespace 
  // デバッグメニュー基底
  public class LuaDebugMenu : MonoBehaviour {
    public delegate void State();
    public delegate void ConfirmFunc();
    //
    const int PORT = 6456;
    const float kRETINA_W = 640.0f;
    const float kRETINA_H = 1136.0f;
    const float kBUTTON_SIZE = 96.0f;
    const float kASPECT_RATIO = kRETINA_W / kRETINA_H;
    //
    static LuaDebugMenu _instance = null;
    //
    bool _silentMode = false;
    Rect _buttonRect;
    float _touched;
    Rect _menuRect;
    GUIStyle _boxStyle = null;
    GUIStyle _windowStyle = null;
    GUIStyle _valueStyle = null;
    GUIStyle _buttonStyle = null;
    GUI.WindowFunction _delegatefunction;
    string _windowTitle;
    string _ipaddr = "192.168.0.1";
    int _addr = 0;
    bool _connected = false;
    string[] _confirmString;
    ConfirmFunc[] _confirmFunction;
    State _state = null;
    State _stateNext = null;
    public State stateNext {
      set {
        _stateNext = value;
      }
    }
    bool _first = false;
    float _interp;
    List<Tab> _tabs = new List<Tab>();
    int _selectTab = -1;
    float _bakScale = 1.0f;
    //
    public bool _permanent = false;
    public Color _valueStyleFG = new Color(1, 1, 1, 1);
    public Texture2D _valueStyleBG;
    //
    public float buttonWidth {
      get { return _buttonRect.width; }
    }
    public float buttonHeight {
      get { return _buttonRect.height; }
    }
    public float halfButtonWidth {
      get { return _buttonRect.width * 0.5f; }
    }
    public float halfButtonHeight {
      get { return _buttonRect.height * 0.5f; }
    }
    public Rect menuWindowRect {
      get { return _menuRect; }
      set { _menuRect = value; }
    }
    public GUI.WindowFunction delegateFunction {
      set { _delegatefunction = value; }
    }
    public string windowTitle {
      get { return _windowTitle; }
      set { _windowTitle = value; }
    }
    public string ipaddr {
      get { return _ipaddr; }
      set { _ipaddr = value; }
    }
    public int addr {
      get { return _addr; }
      set { _addr = value; }
    }
    public bool connected {
      get { return _connected; }
      set { _connected = value; }
    }
    public bool isWakeup {
      get { return _state != state_button; }
    }
    public static LuaDebugMenu instance {
      get { return _instance; }
    }
    public static bool isAlive {
      get { return _instance != null; }
    }
    // タブ
    public abstract class Tab {
      LuaDebugMenu _menu;
      string _name;
      ushort _priority = 1000;
      ushort _order = 0;
      public LuaDebugMenu menu {
        get { return _menu; }
        internal set { _menu = value; }
      }
      public string name {
        get { return _name; }
      }
      protected ushort priority {
        get { return _priority; }
        set { _priority = value; }
      }
      public ushort order {
        get { return _order; }
        internal set { _order = value; }
      }
      public uint sortOrder {
        get { return ((uint)_priority << 16) | ((uint)_order); }
      }
      protected Tab(string n) {
        _name = n;
      }
      public virtual void Init() {}
      public virtual void Start() {}
      public virtual void Stop() {}
      public virtual void Execute() {
        GUILayout.Label(name);
      }
    }
    //
    void Awake() {
      if (_permanent) {
        DontDestroyOnLoad(gameObject);
      }
      if (Debug.isDebugBuild && _instance == null) {
        _instance = this;
        float w = Screen.width / 15.0f;
        float h = Screen.height / 15.0f;
        float sz = Mathf.Max(w, h);
        stateNext = state_button;
        _buttonRect = new Rect(0, 0, sz, sz);
        _touched = 0.0f;
        _menuRect = new Rect(0, 0, Screen.width, Screen.height * 0.9f);
        _delegatefunction = DefaultDelegate;
        windowTitle = "DEBUG MENU";
        _ipaddr = PlayerPrefs.GetString("LuaDebugMenu.ServerAddressString", "192.168.0.1");
        check_connection();
        default_tabs();
        StartCoroutine(proc_server());
      } else {
        Destroy(gameObject);
      }
    }
    void OnDestroy() {
      if (_instance == this) {
        PlayerPrefs.SetString("LuaDebugMenu.ServerAddressString", _ipaddr);
        _instance = null;
      }
    }
    void Start() {
      Debug.Log("super.Start()");
    }
    void default_tabs() {
      AddTab(new LogTab("LOG"));
      AddTab(new ServerTab("SERVER"));
      AddTab(new SystemTab("SYS"));
    }
    void AddTab(Tab t) {
      t.order = (ushort)_tabs.Count;
      t.menu = this;
      _tabs.Add(t);
      sort_tabs();
      t.Init();
      _selectTab = 0;
    }
    void sort_tabs() {
      _tabs.Sort((a, b) => (a.sortOrder < b.sortOrder ? -1 : 1));
    }
    void DefaultDelegate(int id) {
      if (_tabs.Count > 0) {
        int oldtab = _selectTab;
        string[] tabtitle = new string[_tabs.Count];
        for (int i = 0; i < _tabs.Count; ++i) {
          Tab t = _tabs[i];
          tabtitle[i] = t.name;
        }
        if (tabtitle.Length > 4) {
          _selectTab = GUILayout.SelectionGrid(_selectTab, tabtitle, 4, _boxStyle);
        } else {
          _selectTab = GUILayout.Toolbar(_selectTab, tabtitle, _boxStyle);
        }
        GUILayout.Space(Screen.height * 0.01f);
        if (oldtab != _selectTab) {
          if (oldtab >= 0) _tabs[oldtab].Stop();
          if (_selectTab >= 0) _tabs[_selectTab].Start();
        }
        _tabs[_selectTab].Execute();
      }
    }
    public static void Wakeup() {
      instance.stateNext = instance.state_wakeup;
    }
    public static void Shutdown() {
      instance.stateNext = instance.state_shutdown;
    }
    void Confirm(string lstr, ConfirmFunc lfunc, string rstr, ConfirmFunc rfunc) {
      stateNext = state_confirm;
      _confirmString = new string[2];
      _confirmFunction = new ConfirmFunc[2];
      _confirmString[0] = (lstr != null)? lstr : "CANCEL";
      _confirmString[1] = (rstr != null)? rstr : "OK";
      _confirmFunction[0] = lfunc;
      _confirmFunction[1] = rfunc;
    }
    void OnGUI() {
      make_style();
      if (_stateNext != null) {
        _first = true;
        _state = _stateNext;
        _stateNext = null;
      }
      _state();
      _first = false;
    }
    void state_button() {
      if (_first) {
        Debug.Log("BUTTON");
      }
      Event e = Event.current;
      //if (e.type != EventType.Used)
      if (_touched == 0.0f) {
        if (e.type == EventType.MouseDown && _buttonRect.Contains(e.mousePosition)) {
          _touched += Time.deltaTime;
          e.Use();
        } else {
          AdjustRect();
        }
      } else {
        _touched += Time.deltaTime;
        if (_touched < 0.5f) {
          if (e.type == EventType.MouseUp) {
            if (_buttonRect.Contains(e.mousePosition)) {
              stateNext = state_wakeup;
            }
            _touched = 0.0f;
          }
        } else {
          // move
          _buttonRect.x = e.mousePosition.x - _buttonRect.width * 0.5f;
          _buttonRect.y = e.mousePosition.y - _buttonRect.height * 0.5f;
          if (e.type == EventType.MouseUp) {
            _touched = 0.0f;
          }
        }
        if (e.isMouse) {
          e.Use();
        }
      }
      LimitRect();
      if (!_silentMode) RenderButton();
    }
    void state_wakeup() {
      if (_first) {
        Debug.Log("WAKEUP");
        _interp = 0.0f;
        //_bakScale = AppTime.GetScale(0);
        //AppTime.SetScale(0, 0.0f);
      }
      Rect rc = LerpRect(_buttonRect, _menuRect, _interp);
      _interp += (1.0f / 0.25f) * Time.deltaTime;
      if (_interp >= 1.0f) {
        stateNext = state_delegate;
      }
      rc = GUI.Window(0, rc, _delegatefunction/*DefaultDelegate*/, _windowTitle);
      //rc = GUI.Window(0, rc, DefaultDelegate, _windowTitle);
    }
    void state_shutdown() {
      if (_first) {
        Debug.Log("SHUTDOWN");
        _interp = 0.0f;
        //AppTime.SetScale(0, _bakScale);
      }
      Rect rc = LerpRect(_menuRect, _buttonRect, _interp);
      _interp += (1.0f / 0.25f) * Time.deltaTime;
      if (_interp >= 1.0f) {
        stateNext = state_button;
      }
      rc = GUI.Window(0, rc, _delegatefunction/*DefaultDelegate*/, _windowTitle);
      //rc = GUI.Window(0, rc, DefaultDelegate, _windowTitle);
    }
    void state_delegate() {
      Event e = Event.current;
      if (e.type == EventType.MouseDown && !_menuRect.Contains(e.mousePosition)) {
        Shutdown();
        e.Use();
      }
      _menuRect = GUI.Window(0, _menuRect, _delegatefunction, _windowTitle);//, _windowStyle)
    }
    void state_confirm() {
      float buttonwidth = Screen.width / 2;
      float buttonheight = Screen.height / 15;
      GUILayout.BeginHorizontal();
      if (GUILayout.Button(_confirmString[0], GUILayout.Width(buttonwidth), GUILayout.Height(buttonheight))) {
        if (_confirmFunction[0] != null) (_confirmFunction[0])();
         stateNext = state_delegate;
      } else
      if (GUILayout.Button(_confirmString[1], GUILayout.Width(buttonwidth), GUILayout.Height(buttonheight))) {
        if (_confirmFunction[1] != null) (_confirmFunction[1])();
        stateNext = state_delegate;
      }
      GUILayout.EndHorizontal();
    }
    static Rect LerpRect(Rect fromrc, Rect to, float t) {
      Rect rc = new Rect(0, 0, 0, 0);
      rc.x = Mathf.Lerp(fromrc.x, to.x, t);
      rc.y = Mathf.Lerp(fromrc.y, to.y, t);
      rc.width = Mathf.Lerp(fromrc.width, to.width, t);
      rc.height = Mathf.Lerp(fromrc.height, to.height, t);
      return rc;
    }
    void LimitRect() {
      float w = buttonWidth;
      float h = buttonHeight;
      _buttonRect.x = Mathf.Clamp(_buttonRect.x, 0.0f, Screen.width - w);
      _buttonRect.y = Mathf.Clamp(_buttonRect.y, 0.0f, Screen.height - h);
    }
    void AdjustRect() {
      float x = _buttonRect.x + halfButtonWidth;
      float y = _buttonRect.y;
      float nx = (x < Screen.width * 0.5f ? 0.0f : Screen.width);
      float ny = y;
      _buttonRect.x += (nx - x) * 0.07f;
      _buttonRect.y += (ny - y) * 0.07f;
    }
    void RenderButton() {
      GUI.Box(_buttonRect, "DEBUG");//, _boxStyle)
    }
    void make_style() {
      if (_windowStyle == null) {
        _windowStyle = new GUIStyle();
        _windowStyle.fontSize = 20;
        _windowStyle.alignment = TextAnchor.MiddleCenter;
        _windowStyle.normal.textColor = Color.white;
        //_windowStyle.normal.background = Resources.LoadAssetAtPath[of Texture2D]('Assets/Texture/black.png')
      }
      //
      if (_boxStyle == null || _boxStyle.name == "") {
        //fnt = _boxStyle.font
        _boxStyle = new GUIStyle("button");
        //_boxStyle.font = fnt
        Debug.Log("FONTSIZE: ${_boxStyle.fontSize}");
        _boxStyle.fontSize = (int)(Screen.width * 1.0f / 20.0f);//CalcVerticalInRetina(48.0f);
        Debug.Log("FONTSIZE: ${_boxStyle.fontSize}");
      }
      //
      if (_valueStyle == null) {
        _valueStyle = new GUIStyle();
        _valueStyle.fontSize = 20;
        _valueStyle.alignment = TextAnchor.MiddleCenter;
        _valueStyle.normal.textColor = _valueStyleFG;
        _valueStyle.normal.background = _valueStyleBG;//Resources.LoadAssetAtPath[of Texture2D]('Assets/Texture/black.png')
      }
      //
      if (_buttonStyle == null) {
        _buttonStyle = new GUIStyle("button");
        _buttonStyle.fontSize = (int)(Screen.height / 32.0f);
      }
    }
    int tenkey(int v, int lo, int hi) {
      float buttonheight = Screen.height / 10;
      string[][] buttons = new string[][]{
        new string[]{"7", "8", "9"},
        new string[]{"4", "5", "6"},
        new string[]{"1", "2", "3"},
        new string[]{"C", "0", "<"},
      };
      string vs = v.ToString();
      foreach (string[] column in buttons) {
        GUILayout.BeginHorizontal();
        foreach (string str in column) {
          if (str == "C") {
            if (GUILayout.Button(str, _buttonStyle, GUILayout.Height(buttonheight))) {
              vs = "0";
            }
          } else
          if (str == "<") {
            if (GUILayout.Button(str, _buttonStyle, GUILayout.Height(buttonheight))) {
              int l = vs.Length;
              if (l > 0) {
                vs = vs.Remove(l - 1, 1);
                if (string.IsNullOrEmpty(vs)) {
                  vs = "0";
                }
              }
            }
          } else {
            if (GUILayout.Button(str, _buttonStyle, GUILayout.Height(buttonheight))) {
              vs = string.Concat(vs, str);
            }
          }
        }
        GUILayout.EndHorizontal();
      }
      v = int.Parse(vs);
      return Mathf.Clamp(v, lo, hi);
    }
    //
    string make_address() {
#if UNITY_EDITOR
      return string.Format("http://127.0.0.1:{0}/", PORT);
#else
      return string.Format("http://{0}:{1}/", _ipaddr, PORT);
#endif
    }
    void check_connection() {
      WWW www = new WWW(string.Format("{0}api?ping", make_address()));
      while (!www.isDone) {}
      _connected = false;
      if (string.IsNullOrEmpty(www.error)) {
        if (www.text == "ok") {
          _connected = true;
        }
      }
    }
    IEnumerator proc_server() {
      while (true) {
        WWW www = new WWW(string.Format("{0}api?ping", make_address()));
        yield return www;
        _connected = false;
        if (string.IsNullOrEmpty(www.error)) {
          if (www.text == "ok") {
            _connected = true;
          }
        }
        yield return new WaitForSeconds(3);
      }
    }
    //
    public string LoadFile(string path) {
      WWW www = new WWW(string.Format("{0}{1}", make_address(), path));
      while (!www.isDone) {
        // block
      }
      if (string.IsNullOrEmpty(www.error)) {
        return www.text;
      }
#if UNITY_EDITOR
      Debug.Log(www.error);
#endif
      return null;
    }
    //
    public class SystemTab : Tab {
      public SystemTab(string n) : base(n) {}
      public override void Execute() {
        float buttonheight = Screen.height / 15;
        if (GUILayout.Button("RELOAD", GUILayout.Height(buttonheight))) {
          menu.Confirm(null, null, null, reload);
        }
      }
      public void reload() {
        Application.LoadLevel(Application.loadedLevelName);
      }
    }
    //
    public class LogTab : Tab {
      Vector2 _logpos = Vector2.zero;
      static List<string> _logs = new List<string>();
      public LogTab(string n) : base(n) {
        //_logs.Clear();
      }
      public override void Execute() {
        _logpos = GUILayout.BeginScrollView(_logpos);
        string txt = "";
        foreach (string s in _logs) {
          txt += s + "\n";
        }
        GUILayout.Label(txt);
        GUILayout.EndScrollView();
      }
      public static void Add(string txt) {
        if (Debug.isDebugBuild) {
          _logs.Add(txt);
          if (_logs.Count > 300) {
            _logs.RemoveAt(0);
          }
        }
      }
    }
    //
    public class ServerTab : Tab {
      bool _editing = false;
      public ServerTab(string n) : base(n) {}
      public override void Execute() {
        GUILayout.Label(string.Format("STATUS: {0}", menu.connected ? "CONNECTED" : "DISCONNECTED"));
        GUILayout.BeginHorizontal();
        GUILayout.Label("IP ADDRESS:");
        if (_editing) {
          menu.ipaddr = GUILayout.TextField(menu.ipaddr);
          if (GUILayout.Button("SAVE")) {
            _editing = false;
          }
        } else {
          GUILayout.Label(menu.ipaddr);
          if (GUILayout.Button("EDIT")) {
            _editing = true;
          }
        }
        GUILayout.EndHorizontal();
        //GUILayout.Label(string.Format("IP ADDRESS: {0}", menu.addr));
        //menu.addr = menu.tenkey(menu.addr, 0, 254);
      }
    }
  }
}
