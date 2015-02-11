using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace catan {
  //[ExecuteInEditMode]
  public class LWFSpriteObject : MonoBehaviour {
    public enum RenderType {
      Mesh,
      UI,
    }
    public string _lwfName;
    public RenderType _renderType = RenderType.Mesh;
    [HideInInspector] public bool _hidePlayer = true;
    private LWFPlayer _player;
    private GameObject _playerObject;
    private Transform _transform;
    //
    public string lwfName {
      get { return _lwfName; }
      set { _lwfName = value; }
    }
    public LWFPlayer player {
      get { return _player; }
    }
    public RenderType renderType {
      get { return _renderType; }
      set {
        RenderType old = _renderType;
        _renderType = value;
        if (old != _renderType) {
          create_player();
        }
      }
    }
    public bool hidePlayer {
      get { return _hidePlayer; }
      set {
        _hidePlayer = value;
        set_player_visibility(!_hidePlayer);
      }
    }
    //
    void Awake() {
      _transform = transform;
    }
    //
    void Start() {
#if UNITY_EDITOR
      create_player();
#else
      _playerObject = find_player();
      if (_playerObject != null) {
        _player = _playerObject.GetComponent<LWFPlayer>();
      } else {
        create_player();
      }
#endif
    }
    void Update() {
    }
    public void Play(string label) {
      _player.GotoAndPlayMovie("_root", label, true);
    }
    public void CreatePlayer() {
      create_player();
    }
    public bool HasPlayer() {
#if UNITY_EDITOR
      return _player != null || enum_players().Length > 0;
#else
      return _player != null;
#endif
    }
    GameObject find_player() {
      foreach (Transform t in _transform) {
        GameObject obj = t.gameObject;
        if (obj.name.StartsWith("hidden/")) {
          return obj;
        }
      }
      return null;
    }
    GameObject[] enum_players() {
      List<GameObject> objs = new List<GameObject>();
      foreach (Transform t in transform) {
        GameObject obj = t.gameObject;
        if (obj.name.StartsWith("hidden/")) {
          objs.Add(obj);
        }
      }
      return objs.ToArray();
    }
    void create_player() {
      destroy_player();
      GameObject obj = new GameObject("hidden/" + gameObject.name);
      obj.transform.SetParent(transform, false);
      _player = obj.AddComponent<LWFPlayer>();
      _player.lwfName = _lwfName;
      _player.renderType = _renderType;
      _playerObject = obj;
#if UNITY_EDITOR
      set_player_visibility(!_hidePlayer);
#endif
    }
    void destroy_player() {
#if UNITY_EDITOR
      GameObject[] objs = enum_players();
      foreach (GameObject obj in objs) {
        DestroyImmediate(obj);
      }
#else
      if (HasPlayer()) {
        Destroy(_player.gameObject);
      }
#endif
      _player = null;
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void set_player_visibility(bool f) {
      GameObject[] objs = enum_players();
      foreach (GameObject obj in objs) {
        if (obj.name.StartsWith("hidden/")) {
          if (f) {
            obj.hideFlags = HideFlags.None;
          } else {
            obj.hideFlags = HideFlags.HideInInspector;
          }
        }
      }
    }
  }



}
