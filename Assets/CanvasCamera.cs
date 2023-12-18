using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCamera : MonoBehaviour {
    //public Camera camera;
    // Start is called before the first frame update
    void Start() {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
