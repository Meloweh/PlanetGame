using System.Collections;
using UnityEngine;

public class SandSimulation : MonoBehaviour {
    public ComputeShader SandComputeShader;
    public GameObject sampe;
    [SerializeField] Texture2D output = null;
    [SerializeField] Texture texture = null;
 
    [SerializeField] Color overlay = Color.white;
 
    private RenderTexture final;
    private Texture2D generated;
 
    private int kernel;
    private bool hasKernel = false;
    private Color applied = Color.white;
    void Start()
    {
        kernel   = SandComputeShader.FindKernel("CSMain");
        hasKernel = SandComputeShader.HasKernel("CSMain");
 
        generated = new Texture2D(texture.width, texture.height);
 
        final = new RenderTexture(texture.width, texture.height, 24)
        {
            enableRandomWrite = true
        };
        final.Create();
    }
 
    void Update()
    {
        if (!hasKernel) return;
 
        bool hasChanged = overlay != applied;
 
        SandComputeShader.SetTexture(kernel, "Result", final);
        SandComputeShader.SetTexture(kernel, "ImageInput", texture);
        SandComputeShader.SetVector("color", overlay);
        SandComputeShader.Dispatch(kernel, texture.width / 8, texture.height / 8, 1);      
 
        if (hasChanged)
        {
            RenderTexture.active = final;
 
            //Expensive part
            generated.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            generated.Apply();
 
            SpriteRenderer renderer = sampe.GetComponent<SpriteRenderer>();
            if (renderer == null) {
                Debug.print("No renderer for sample");
            } else {
                Sprite newSprite = Sprite.Create(generated, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 10.0f);
                renderer.sprite = newSprite;
            }
        }
 
        applied = overlay;
    }
}