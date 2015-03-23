using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UniLua;

namespace catan {
  [CustomEditor(typeof(LuaScriptBehaviour))]
  public class LuaScriptBehaviourEditor : Editor {
    private LuaScriptBehaviour _lsb;
    private GameObject _gameObject;
    private FileList _fileList;
    private int _fileSelectIndex;
    //
    void OnEnable() {
      _lsb = (LuaScriptBehaviour)target;
      _gameObject = _lsb.gameObject;
      _fileList = new FileList("StreamingAssets/LuaRoot");
      _fileSelectIndex = _fileList.Find(_lsb.luaScriptFile);
    }
    void OnDisable() {
      _lsb = null;
      _gameObject = null;
    }
    public override void OnInspectorGUI() {
      GUILayout.Label("Lua Script File");
      string name = _lsb.luaScriptFile;
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.Space();
        name = EditorGUILayout.TextField("Filename", name);
      }
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.Space();
        int idx = EditorGUILayout.Popup(_fileSelectIndex, _fileList.files);
        if (_fileSelectIndex != idx) {
          _fileSelectIndex = idx;
          name = _fileList.files[idx];
        }
      }
      EditorGUILayout.EndHorizontal();
      if (GUI.changed) {
        _lsb.luaScriptFile = name;
        EditorUtility.SetDirty(target);
      }
    }
    internal class FileList {
      private string _basedir;
      private string[] _files;
      public string[] files {
        get { return _files; }
      }
      public FileList(string basedir) {
        _basedir = Application.dataPath + "/" + basedir;
        if (System.IO.Directory.Exists(_basedir)) {
          _files = collect_files_recursive(make_path(""), "", ".lua").ToArray();
        } else {
          _files = new string[0];
        }
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
          entries.Add(s.Remove(0, root.Length));
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
}
