using UnityEngine;
using System.Collections;

public class LWFSpriteObject : MonoBehaviour {
  public string _lwfName;
  [HideInInspector] public LWFPlayer _lwfPlayer;
  [HideInInspector] public bool _hidePlayer = true;
  private Transform _transform;
  //
  public string lwfName {
    get { return _lwfName; }
    set { _lwfName = value; }
  }
  public LWFPlayer player {
    get { return _lwfPlayer; }
  }
  public bool hidePlayer {
    get { return _hidePlayer; }
    set {
      _hidePlayer = value;
#if UNITY_EDITOR
      set_player_visibility(!_hidePlayer);
#endif
    }
  }
  //
  void Awake() {
    _transform = transform;
  }
  //
  void Start() {
    if (!HasLWFPlayer()) {
      CreatePlayer();
    }
  }
  void Update() {
  }
  public void Play(string label) {
    _lwfPlayer.GotoAndPlayMovie("_root", label, true);
  }
  public void CreatePlayer() {
    create_lwfobj();
  }
  public bool HasLWFPlayer() {
    return _lwfPlayer != null;
  }
  void create_lwfobj() {
    destroy_lwfobj();
    GameObject obj = new GameObject("hidden/" + gameObject.name);
    obj.transform.SetParent(transform, false);
    _lwfPlayer = obj.AddComponent<LWFPlayer>();
    _lwfPlayer.lwfName = _lwfName;
#if UNITY_EDITOR
    set_player_visibility(!_hidePlayer);
#endif
  }
  void destroy_lwfobj() {
    if (HasLWFPlayer()) {
#if UNITY_EDITOR
      DestroyImmediate(_lwfPlayer.gameObject);
#else
      Destroy(_lwfPlayer.gameObject);
#endif
      _lwfPlayer = null;
    }
  }
#if UNITY_EDITOR
  public void set_player_visibility(bool f) {
    foreach (Transform child in transform) {
      GameObject obj = child.gameObject;
      if (obj.name.IndexOf("hidden/") == 0) {
        if (f) {
          obj.hideFlags = HideFlags.None;
        } else {
          obj.hideFlags = HideFlags.HideInHierarchy;
        }
      }
    }
  }
#endif
}
