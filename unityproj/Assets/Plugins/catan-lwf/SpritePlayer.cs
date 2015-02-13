using UnityEngine;
using System.Collections;

namespace catan {
  namespace lwf {
    using RenderType = SpriteObject.RenderType;
    [ExecuteInEditMode, AddComponentMenu("")]
    public partial class SpritePlayer : LWFObject {
      private static bool _setLoader = false;
      //
      [HideInInspector] public string _lwfName;
      [HideInInspector] public string _texDir;
      [HideInInspector] public RenderType _renderType = RenderType.Mesh;
      //
      public string lwfName {
        get {
          return _lwfName;
        }
        set {
          set_resource_name(value);
        }
      }
      public RenderType renderType {
        set { _renderType = value; }
      }
      public LWF.Movie rootMovie {
        get { return lwf.rootMovie; }
      }
      //
      void Awake() {
        //_lwfobj = GetComponentInChildren<LwfSampleObject>();
        //this.shaderName = "Oink/LWF";
      }
      void Start() {
        //GotoAndPlayMovie("_root", 1, true);
        load_lwf();
      }
      //
      //public void GotoAndPlayMovie(string instancename, string labelname, bool immortal=false) {
      //  GotoAndPlay(labelname);
      //  lwf.rootMovie.GotoAndPlay(labelname);
      //}
      //
      private void load_lwf() {
        set_loader();
        switch (_renderType) {
        case RenderType.Mesh:
#if UNITY_EDITOR
          UseDrawMeshRenderer();
#else
          UseCombinedMeshRenderer();
#endif
          break;
        case RenderType.UI:
          clear_mesh_components();
          UseUIVertexRenderer();
          break;
        }
        Load(_lwfName, _texDir);//, shaderName="Oink/LWF");
        //AttachLWF("_root", _lwfName, "_root");
        //var labels = lwf.rootMovie.GetCurrentLabels();
        //foreach (LWF.LabelData data in labels) {
        //  Debug.Log(string.Format("{0} {1}", data.frame, data.name));
        //}
      }
      //
      private void set_resource_name(string name) {
        _lwfName = name;
        _texDir = System.IO.Path.GetDirectoryName(name);
        if (_texDir.Length > 0) {
          _texDir += "/";
        }
      }
      [System.Diagnostics.Conditional("UNITY_EDITOR")]
      private void clear_mesh_components() {
        foreach (Transform t in transform) {
          GameObject obj = t.gameObject;
          if (obj.name.StartsWith("LWF/")) {
            GameObject.DestroyImmediate(obj);
          }
        }
      }
      private static void set_loader() {
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
  } // namespace lwf
} // namespace catan
