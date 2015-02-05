using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LWFPlayer : LWFObject {
  private static bool _setLoader = false;
  [HideInInspector] public string _lwfName;
  [HideInInspector] public string _texDir;
  public string lwfName {
    get {
      return _lwfName;
    }
    set {
      set_resource_name(value);
    }
  }
  public LWF.Movie rootMovie {
    get { return lwf.rootMovie; }
  }
  //
  void Awake() {
    //_lwfobj = GetComponentInChildren<LwfSampleObject>();
#if UNITY_EDITOR
    //UseDrawMeshRenderer();
#endif
  }
  void Start() {
    //GotoAndPlayMovie("_root", 1, true);
    load_lwf();
    //Load("lwfdata/character", "lwfdata/");
  }
  //
  //public void GotoAndPlayMovie(string instancename, string labelname, bool immortal=false) {
  //  GotoAndPlay(labelname);
  //  lwf.rootMovie.GotoAndPlay(labelname);
  //}
  //
  private void load_lwf() {
    set_loader();
    Load(_lwfName, _texDir);
    var labels = lwf.rootMovie.GetCurrentLabels();
    foreach (LWF.LabelData data in labels) {
      Debug.Log(string.Format("{0} {1}", data.frame, data.name));
    }
  }
  //
  private void set_resource_name(string name) {
    _lwfName = name;
    _texDir = System.IO.Path.GetDirectoryName(name);
    if (_texDir.Length > 0) {
      _texDir += "/";
    }
  }
  public static void set_loader() {
    if (!_setLoader) {
      LWFObject.SetLoader(
        lwfDataLoader:(name) => {
          TextAsset asset = Resources.Load(name) as TextAsset;
          if (asset == null) {
            return null;
          }
          return asset.bytes;
        },
        textureLoader:(name) => {
          Texture2D texture = Resources.Load(name) as Texture2D;
          if (texture == null) {
            return null;
          }
          return texture;
        }
      );
      _setLoader = true;
    }
  }

}
