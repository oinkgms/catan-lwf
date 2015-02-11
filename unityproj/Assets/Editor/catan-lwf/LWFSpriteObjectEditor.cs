using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace catan {
  [CustomEditor(typeof(LWFSpriteObject))]
  public class LWFSpriteObjectEditor : Editor {
    private LWFSpriteObject _lwfObject;
    private GameObject _gameObject;
    private LWFSpriteFileList _fileList;
    private int _fileSelectIndex;
    //
    void OnEnable() {
      _lwfObject = (LWFSpriteObject)target;
      _gameObject = _lwfObject.gameObject;
      _fileList = new LWFSpriteFileList("Resources");
      _fileSelectIndex = _fileList.Find(_lwfObject.lwfName);
      if (!_lwfObject.HasPlayer()) {
        check_and_load(_lwfObject.lwfName);
      }
    }
    void OnDisable() {
      _lwfObject = null;
      _gameObject = null;
    }
    public override void OnInspectorGUI() {
      GUILayout.Label("LWF Name");
      string name = _lwfObject.lwfName;
      //
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.Space();
        name = EditorGUILayout.TextField("filename", name);
      }
      EditorGUILayout.EndHorizontal();
      //
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.Space();
        int idx = EditorGUILayout.Popup(_fileSelectIndex, _fileList.files);
        if (_fileSelectIndex != idx) {
          _fileSelectIndex = idx;
          name = _fileList.files[idx];
          EditorUtility.SetDirty(target);
        }
      }
      EditorGUILayout.EndHorizontal();
      //
      EditorGUILayout.BeginHorizontal();
      {
        _lwfObject.renderType = (LWFSpriteObject.RenderType)EditorGUILayout.EnumPopup("Renderer", _lwfObject.renderType);
      }
      EditorGUILayout.EndHorizontal();


      if (name != _lwfObject.lwfName) {
        check_and_load(name);
      }
      if (!exist_resource(name)) {
        EditorGUILayout.HelpBox("missing LWF", MessageType.Error);
      } else {
        EditorGUILayout.BeginHorizontal();
        bool f = EditorGUILayout.ToggleLeft("Hide Player", _lwfObject.hidePlayer);
        if (f != _lwfObject.hidePlayer) {
          _lwfObject.hidePlayer = f;
          UnityEditor.EditorApplication.RepaintHierarchyWindow();
        }
        if (GUILayout.Button("FORCE RELOAD")) {
          check_and_load(name);
        }
        EditorGUILayout.EndHorizontal();
      }
      if (GUI.changed) {
        EditorUtility.SetDirty(target);
      }
    }
    //
    public static bool exist_resource(string name) {
      return System.IO.File.Exists(Application.dataPath + "/Resources/" + name + ".bytes");
    }
    //
    private void check_and_load(string name) {
      _lwfObject.lwfName = name;
      _fileSelectIndex = _fileList.Find(name);
      if (exist_resource(name)) {
        Debug.Log(name);
        _lwfObject.CreatePlayer();
      }
    }
  }

  internal class LWFSpriteFileList {
    private string _basedir;
    private string[] _files;
    public string[] files {
      get { return _files; }
    }
    public LWFSpriteFileList(string basedir) {
      _basedir = Application.dataPath + "/" + basedir;
      _files = collect_files_recursive(make_path(""), "", ".bytes").ToArray();
    }
    public int Find(string name) {
      return Array.IndexOf(_files, name);
    }
    private List<string> collect_files_recursive(string root, string path, string ext) {
      string[] subdirs = filter_ignore_entries(Directory.GetDirectories(make_path(path)));
      List<string> entries = new List<string>();
      foreach (string s in subdirs) {
        string s2 = s.Remove(0, root.Length);
        entries.AddRange(collect_files_recursive(root, s2, ext));
      }
      string[] rawfiles = filter_ignore_entries(Directory.GetFiles(make_path(path)));
      ext = ext.ToLower();
      string[] exts = ext.Split('|');
      rawfiles =
        Array.FindAll(rawfiles,
          delegate(string name) {
          return
          Array.IndexOf(exts, Path.GetExtension(name).ToLower()) >= 0;
          });
      foreach (string s in rawfiles) {
        entries.Add(s.Remove(0, root.Length).Replace(".bytes", ""));
      }
      return entries;
    }
    public string[] collect_files(string path) {
      string[] rawfiles = Directory.GetFiles(make_path(path));
      string[] files = new string[rawfiles.Length];
      int idx = 0;
      foreach (string s in rawfiles)
        files[idx++] = Path.GetFileName(s);
      return filter_ignore_entries(files);
    }
    // extに拡張子を与えると該当するファイルのみ取得します
    // 拡張子リストを | で区切ると複数指定できます -> ".fbx|.prefab"
    public string[] collect_files(string path, string ext) {
      string[] files = collect_files(path);
      ext = ext.ToLower();
      string[] exts = ext.Split('|');
      string[] filtered =
        Array.FindAll(files,
          delegate(string name) {
          return
          Array.IndexOf(exts, Path.GetExtension(name).ToLower()) >= 0;
          });
      return filtered;
    }
    private string make_path(string path) {
      return _basedir + "/" + path;
    }
    private static string[] filter_ignore_entries(string[] files) {
      return
        Array.FindAll(files,
          delegate(string name) {
          return
          Path.GetFileName(name).IndexOf('.') != 0 &&
          Path.GetExtension(name).ToLower() != ".meta";
          });
    }


  }



}
