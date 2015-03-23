using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;

namespace catan {
  public class LuaEditorMenu {

    [MenuItem("catan/UniLua/Build All")]
    public static void BuildScripts() {
      build_lua_scripts("StreamingAssets/LuaRoot", ".");
      Debug.Log(Directory.GetCurrentDirectory());



    }

    private static void build_lua_scripts(string root, string path) {
      string[] files = get_files(root + "/" + path, ".lua");
      foreach (string f in files) {
        build_lua_script(root, path + "/" + f);
      }
      string[] dirs = get_dirs(root + "/" + path);
      foreach (string d in dirs) {
        build_lua_scripts(root, path + "/" + d);
      }
    }
    private static void build_lua_script(string root, string file) {
      string binroot = "Assets/" + root.Replace("LuaRoot", "LuaBinRoot");
      string binfile = (binroot + "/" + file).Replace("./", "");
      string bindir = Path.GetDirectoryName(binfile);
      Debug.Log(string.Format("{0} -> {1}", file, binfile));
      if (!Directory.Exists(bindir)) {
        Directory.CreateDirectory(bindir);
        AssetDatabase.ImportAsset(bindir, ImportAssetOptions.Default);
      }
      //UniLua.Tools.Compiler.ListingToFile(file, file);
      UniLua.Tools.Compiler.DumpingToFile(file, binfile, true);
      AssetDatabase.ImportAsset(binfile, ImportAssetOptions.Default);
    }

    private static string[] get_dirs(string path) {
      string[] rawfiles = Directory.GetDirectories(make_path(path));
      string[] files = new string[rawfiles.Length];
      int idx = 0;
      foreach (string s in rawfiles)
        files[idx++] = Path.GetFileName(s);
      return files;
    }
    private static string[] get_files(string path) {
      string[] rawfiles = Directory.GetFiles(make_path(path));
      string[] files = new string[rawfiles.Length];
      int idx = 0;
      foreach (string s in rawfiles)
        files[idx++] = Path.GetFileName(s);
      return filter_ignore_entries(files);
    }
    // extに拡張子を与えると該当するファイルのみ取得します
    // 拡張子リストを | で区切ると複数指定できます -> ".fbx|.prefab"
    private static string[] get_files(string path, string ext) {
      string[] files = get_files(path);
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
    private static string make_path(string path) {
      return Application.dataPath + "/" + path;
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
