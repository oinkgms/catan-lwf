using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace catan {
  namespace lwf {
    //[ExecuteInEditMode]
    public class SpriteObject : MonoBehaviour {
      public enum RenderType {
        Mesh,
        UI,
      }
      public string _lwfName;
      public RenderType _renderType = RenderType.Mesh;
      private SpritePlayer _player;
      private Transform _transform;
      //
      public string lwfName {
        get { return _lwfName; }
        set { _lwfName = value; }
      }
      public SpritePlayer player {
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
      public bool hasPlayer {
        get {
#if UNITY_EDITOR
          find_player();
#endif
          return _player != null;
        }
      }
      //
      void Awake() {
        _transform = transform;
        find_player();
      }
      //
      void Start() {
        if (!hasPlayer) {
          create_player();
        }
      }
      void Update() {
      }
      public void Play(string label) {
        _player.GotoAndPlayMovie("_root", label);
      }
      public void Stop() {
        _player.StopMovie("_root");
      }
      public void CreatePlayer() {
        create_player();
      }
      SpritePlayer find_player() {
        _player = GetComponent<SpritePlayer>();
        return _player;
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
        _player = gameObject.AddComponent<SpritePlayer>();
        _player.lwfName = _lwfName;
        _player.renderType = _renderType;
      }
      void destroy_player() {
        if (hasPlayer) {
#if UNITY_EDITOR
          DestroyImmediate(_player);
#else
          Destroy(_player);
#endif
        }
        _player = null;
      }
    }
  } // namespace lwf
} // namespace catan
