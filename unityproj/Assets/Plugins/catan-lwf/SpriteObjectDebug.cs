using UnityEngine;
using System.Collections;
using System.Linq;

namespace catan {
  namespace lwf {
    public class SpriteObjectDebug : MonoBehaviour {
      static SpriteObjectDebug _debugTarget = null;
      static SpriteObjectDebug[] _debugObjects = null;
      static int _debugObjectIndex = -1;
      SpriteObject _sprite;
      //
      public SpriteObject sprite {
        get { return _sprite; }
      }
      // Use this for initialization
      void Start() {
        _sprite = gameObject.GetComponent<SpriteObject>();
      }

      // Update is called once per frame
      void Update() {
        _debugObjects = (SpriteObjectDebug[])FindObjectsOfType(typeof(SpriteObjectDebug));
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
            SpriteObjectDebug old = _debugTarget;
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
  } // namespace lwf
} // namespace catan
