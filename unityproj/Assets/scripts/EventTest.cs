using UnityEngine;
using System.Collections;
using catan.lwf;

public class EventTest : MonoBehaviour {
  private bool _attached = false;
  // Use this for initialization
  void Start() {
    load_lwf(gameObject, "lwfdata/1a");
    //StartCoroutine(coro());
  }

  // Update is called once per frame
  void Update() {




  }

  IEnumerator coro() {
    yield return null;
    SpriteObject sobj = GetComponent<SpriteObject>();
    sobj.player.rootMovie.SetEventHandler("finish", () => { Debug.Log("FINISH"); });
  }



  static void handler(LWF.Movie movie, LWF.Button button) {
    Debug.Log("FINISH");
  }




  static SpriteObject load_lwf(GameObject obj, string path) {
    SpriteObject sobj = obj.AddComponent<SpriteObject>();
    sobj.lwfName = path;
    sobj.CreatePlayer();
    sobj.player.AddEventHandler("finish", (movie, button) => { Debug.Log("FINISH"); });
    //sobj.player.rootMovie.SetEventHandler("finish", (movie, button) => { Debug.Log("FINISH"); });


    return sobj;
  }




}
