using UnityEngine;
using System.Collections;

namespace catan {
  namespace lwf {
    using RenderType = SpriteObject.RenderType;
    [ExecuteInEditMode, AddComponentMenu("")]
    public partial class SpritePlayer : LWFObject {
      private static bool _globalInitialized = false;
      private static int _multColorId, _addColorId, _lerpColorId;
      //
      [HideInInspector] public string _lwfName;
      [HideInInspector] public string _texDir;
      [HideInInspector] public RenderType _renderType = RenderType.Mesh;
      //
      private int _initialFrameRate;
      private Color _multColor = new Color(1, 1, 1, 1);
      private Color _addColor = new Color(0, 0, 0, 0);
      private Color _lerpColor = new Color(1, 1, 1, 0);
      private bool _dirty = true;
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
      public Color multColor {
        get { return _multColor; }
        set { _multColor = value; _dirty = true; }
      }
      public Color addColor {
        get { return _addColor; }
        set { _addColor = value; _dirty = true; }
      }
      public Color lerpColor {
        get { return _lerpColor; }
        set { _lerpColor = value; _dirty = true; }
      }
      public void ClearColorAdjustment() {
        _multColor = new Color(1, 1, 1, 1);
        _addColor = new Color(0, 0, 0, 0);
        _lerpColor = new Color(1, 1, 1, 0);
        _dirty = true;
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
      void Update() {
        if (_dirty) {
          materialProperty.Clear();
          materialProperty.AddColor(_lerpColorId, _lerpColor);
          materialProperty.AddColor(_addColorId, _addColor);
          materialProperty.AddColor(_multColorId, _multColor);
          _dirty = false;
        }
        base.Update();
      }
      public void SetFrameRate(int fps) {
        lwf.SetFrameRate(fps);
      }
      public void SetPlaySpeed(float spd) {
        lwf.SetFrameRate((int)(_initialFrameRate * spd));
      }
      //
      //public void GotoAndPlayMovie(string instancename, string labelname, bool immortal=false) {
      //  GotoAndPlay(labelname);
      //  lwf.rootMovie.GotoAndPlay(labelname);
      //}
      //
      private void load_lwf() {
        global_init();
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
        Load(_lwfName, _texDir, useAdditionalColor:true, shaderName:"Oink/LWF");
        _initialFrameRate = lwf.frameRate;
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
      private static void global_init() {
        if (!_globalInitialized) {
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
          _multColorId = Shader.PropertyToID("_MultColor");
          _addColorId = Shader.PropertyToID("_AddColor");
          _lerpColorId = Shader.PropertyToID("_LerpColor");
          _globalInitialized = true;
        }
      }
    }
  } // namespace lwf
} // namespace catan
