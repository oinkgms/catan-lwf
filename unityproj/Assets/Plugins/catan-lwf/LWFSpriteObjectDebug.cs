using UnityEngine;
using System.Collections;
using System.Linq;

namespace catan {
  public class LWFSpriteObjectDebug : MonoBehaviour {
    static LWFSpriteObjectDebug _debugTarget = null;
    static LWFSpriteObjectDebug[] _debugObjects = null;
    static int _debugObjectIndex = -1;
    LWFSpriteObject _sprite;
    //
    public LWFSpriteObject sprite {
      get { return _sprite; }
    }
    // Use this for initialization
    void Start() {
      _sprite = gameObject.GetComponent<LWFSpriteObject>();
    }

    // Update is called once per frame
    void Update() {
      _debugObjects = (LWFSpriteObjectDebug[])FindObjectsOfType(typeof(LWFSpriteObjectDebug));
    }
    void OnGUI() {
      float width = Screen.width / 6.0f;
      float height = Screen.height / 8.0f;
      if (_debugObjects != null && (_debugTarget == this || _debugTarget == null)) {
        string[] names = _debugObjects.Select(p => p.gameObject.name).ToArray();
        GUILayout.BeginHorizontal();
        int sel = GUILayout.Toolbar(_debugObjectIndex, names, GUILayout.Height(height));
        GUILayout.EndHorizontal();
        if (sel != _debugObjectIndex) {
          LWFSpriteObjectDebug old = _debugTarget;
          _debugObjectIndex = sel;
          _debugTarget = _debugObjects[_debugObjectIndex];
          if (old != null) old.SendMessage("DebugDeselected", null, SendMessageOptions.DontRequireReceiver);
          _debugTarget.SendMessage("DebugSelected", null, SendMessageOptions.DontRequireReceiver);
        }
      }
      if (_sprite && _debugTarget == this) {
        var labels = _sprite.player.rootMovie.GetCurrentLabels();
        foreach (LWF.LabelData data in labels) {
          if (GUILayout.Button(data.name, GUILayout.Height(height), GUILayout.Width(width))) {
            _sprite.Play(data.name);
          }
        }
      }



    }
  }
}
