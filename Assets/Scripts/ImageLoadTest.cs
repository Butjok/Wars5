using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoadTest : MonoBehaviour {
    public Image image;
    public int index = -1;
    public List<string> paths =new() {
        "/Users/butjok/vfedotov.com/playdead/11.PNG",
        "/Users/butjok/vfedotov.com/playdead/20.PNG",
    };
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)&&paths.Count>0) {
            index = (index + 1) % paths.Count;
            var texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(paths[index]), true);
            image.sprite =  Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), Vector2.zero);
        }
    }
}