using UnityEngine;
using System.Collections;
using catan.lwf;

public class AttachTest : MonoBehaviour {
  private bool _attached = false;
  // Use this for initialization
  void Start() {
  }

  // Update is called once per frame
  void Update() {
    if (!_attached) {
      SpriteObject obj = GetComponent<SpriteObject>();
      obj.player.GotoAndPlayMovie("_root", "melee");
      //Debug.Log(mov);
      //.AttachLWF("lwfdata/lwf_sample", "leg");

      //obj.player.rootMovie.Inspect(inspect, 0, 0, 0);





      _attached = true;
    }




  }
  static void inspect(LWF.Object obj, int h, int d, int r) {
    LWF.Movie mov = obj as LWF.Movie;
    Debug.Log(mov.name);
  }
}
