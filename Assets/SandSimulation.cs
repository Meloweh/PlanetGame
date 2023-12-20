using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class SandSimulation : MonoBehaviour {
    public ComputeShader SandComputeShader;
    private RenderTexture simulationTexture;
    public GameObject sampe;
    public Texture2D tex, myTexture;
    public RenderTexture renderTexture;
    
    private int addSandKernelHandle;
    private int fallSandKernelHandle;
    private int transformKernelHandle;
    private int addPlanetKernelHandle;
    private int captureKernelHandle;
    private int convertTextureToParticlesHandle;

    private Rect areaToCapture;

    public RawImage rawImageDisplay;
    public Canvas canvas;
    struct Particle {
        public Vector4 color;
        public float2 velocity;
        public uint type;
        public int stickyToId;
        public uint mass;
        public float2 position;
        public int3 neighbor;
        public uint pressure;
        public uint falling;
    }
    
    struct Planet {
        public uint id;
        public float2 pos;
        public uint mass;
        public uint active;
    }

    private ComputeBuffer particleBuffer, planetBuffer, rngBuffer, atomicCounter, pressureBuffer;

    private Particle[] particleData;
    private Planet[] planetData;
    private int[] pressureData;
    
    const int planetCount = 32;
    const int rngCount = 32;
    int candidate = -1;

    private RenderTexture areaTexture;

    //private Random random = new Random();

    private void Start() {
        int width = (int)canvas.pixelRect.width;
        int height = (int)canvas.pixelRect.height;

        simulationTexture = new RenderTexture(width, height, 24);
        simulationTexture.enableRandomWrite = true;
        simulationTexture.Create();
        rawImageDisplay.texture = simulationTexture;
        
        SandComputeShader.SetInt("_Width", simulationTexture.width);
        SandComputeShader.SetInt("_Height", simulationTexture.height);

        SandComputeShader.SetInt("_rngCount", rngCount);
        SandComputeShader.SetInt("candidate", candidate);

        addSandKernelHandle = SandComputeShader.FindKernel("CSAddSand");
        fallSandKernelHandle = SandComputeShader.FindKernel("CSFallSand");
        transformKernelHandle = SandComputeShader.FindKernel("CSTransformToTexture");
        addPlanetKernelHandle = SandComputeShader.FindKernel("CSAddPlanet");
        captureKernelHandle = SandComputeShader.FindKernel("CSCaptureArea");
        convertTextureToParticlesHandle = SandComputeShader.FindKernel("CSConvertTextureToParticles");

        rngBuffer = new ComputeBuffer(rngCount, sizeof(uint));

        particleBuffer = new ComputeBuffer(simulationTexture.width * simulationTexture.height, 64);//44
        particleData = new Particle[simulationTexture.width * simulationTexture.height];
        for (int i = 0; i < particleData.Length; i++) {
            particleData[i].color = new Vector4(0, 0, 0, 1);
            particleData[i].velocity = Vector2.zero;
            particleData[i].type = 0;
            particleData[i].stickyToId = -1; // Initialize to 0
            particleData[i].mass = 0;
            particleData[i].position = Vector2.zero;
            particleData[i].neighbor = new int3(-1,-1,-1);
            particleData[i].pressure = 0;
            particleData[i].falling = 0;
        }
        particleBuffer.SetData(particleData);

        SandComputeShader.SetBuffer(addSandKernelHandle, "Particles", particleBuffer);
        SandComputeShader.SetBuffer(fallSandKernelHandle, "Particles", particleBuffer);
        SandComputeShader.SetBuffer(transformKernelHandle, "Particles", particleBuffer);
        SandComputeShader.SetBuffer(addPlanetKernelHandle, "Particles", particleBuffer);

        atomicCounter = new ComputeBuffer(1, sizeof(uint));
        atomicCounter.SetData(new uint[] { 0 });
        SandComputeShader.SetBuffer(addPlanetKernelHandle, "atomicCounter", atomicCounter);

        SandComputeShader.SetInt("_planetCount", planetCount);

        planetBuffer = new ComputeBuffer(planetCount, 20);
        planetData = new Planet[planetCount];

        for (uint i = 1; i < planetData.Length; i++) {
            planetData[i].id = i;
            planetData[i].mass = 0;
            planetData[i].pos = Vector2.zero;
            planetData[i].active = 0;
        }
        planetBuffer.SetData(planetData);

        const int pressureSize = 4 * 4;
        pressureBuffer = new ComputeBuffer(pressureSize, sizeof(int));
        pressureData = new int[pressureSize];
        pressureBuffer.SetData(pressureData);
        SandComputeShader.SetBuffer(fallSandKernelHandle, "pressureIndices", pressureBuffer);
        
        SandComputeShader.SetBuffer(fallSandKernelHandle, "planets", planetBuffer);
        SandComputeShader.SetBuffer(addPlanetKernelHandle, "planets", planetBuffer);

        areaToCapture = new Rect(300, 300, 600, 600);
        Vector4 areaToCaptureVec = new Vector4(areaToCapture.x, areaToCapture.y, areaToCapture.width, areaToCapture.height);
        SandComputeShader.SetVector("areaToCapture", areaToCaptureVec);
        SandComputeShader.SetBuffer(captureKernelHandle, "Particles", particleBuffer);
        SandComputeShader.SetBuffer(convertTextureToParticlesHandle, "Particles", particleBuffer);
        
        areaTexture = new RenderTexture((int)areaToCapture.width, (int)areaToCapture.height, 24);
        areaTexture.enableRandomWrite = true;
        areaTexture.Create();

        myTexture = new Texture2D(150, 150, TextureFormat.RGB24, false);
        for (int x = 0; x < myTexture.width; x++)
        {
            for (int y = 0; y < myTexture.height; y++)
            {
                // Set the pixel to yellow
                myTexture.SetPixel(x, y, Color.yellow);
            }
        }
        myTexture.Apply();

        //copy to rendertexture
        RenderTexture.active = areaTexture;
        //Graphics.Blit(tex, renderTexture);

        SandComputeShader.SetTexture(captureKernelHandle, "AreaTexture", areaTexture);
        //SandComputeShader.SetTexture(convertTextureToParticlesHandle, "AreaTexture", areaTexture);

        SandComputeShader.SetTexture(transformKernelHandle, "ResultTexture", simulationTexture);
    }

    Texture2D ConvertRenderTextureToTexture2D(RenderTexture rTex) {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
    
    private bool capturing;
    void DoCapture() {
        if (capturing) return;
        capturing = true;
        
        //SandComputeShader.Dispatch(captureKernelHandle, (int)areaToCapture.width, (int)areaToCapture.height, 1);
        
        IEnumerator DoReadbackCoroutine() {
            //SandComputeShader.Dispatch(kernelHandle1, ...);
            //SandComputeShader.Dispatch(kernelHandle2, ...);
            SandComputeShader.Dispatch(captureKernelHandle, (int)areaToCapture.width, (int)areaToCapture.height, 1);

            bool isReadbackComplete = false;
            AsyncGPUReadback.Request(areaTexture, 0, data => {

                if (sampe != null && areaTexture != null) {
                    //SandComputeShader.SetTexture(convertTextureToParticlesHandle, "AreaTexture", areaTexture);
                    SpriteRenderer renderer = sampe.GetComponent<SpriteRenderer>();
                    if (renderer == null) {
                        Debug.print("No renderer for sample");
                    } else {
                        Texture2D tx = ConvertRenderTextureToTexture2D(areaTexture);
                        Sprite newSprite = Sprite.Create(tx, new Rect(0.0f, 0.0f, tx.width, tx.height), new Vector2(0.5f, 0.5f));
                        renderer.sprite = newSprite;
                        
                        //renderer.material.mainTexture = areaTexture;
                        Debug.print("Set texture?");
                        
                        //Graphics.CopyTexture(areaTexture, tx);
                        //SandComputeShader.SetTexture(convertTextureToParticlesHandle, "InputTexture", areaTexture);
                        //SandComputeShader.Dispatch(convertTextureToParticlesHandle, areaTexture.width / 8, areaTexture.height / 8, 1);
                    }
                }

                capturing = false;
                isReadbackComplete = true;
                //SandComputeShader.SetVector("PastePosition", new Vector2(0, 500));
                //SandComputeShader.SetVector("AreaSize", new Vector2(areaTexture.width, areaTexture.height));
                //SandComputeShader.SetTexture(convertTextureToParticlesHandle, "InputTexture", areaTexture);
                //SandComputeShader.Dispatch(convertTextureToParticlesHandle, areaTexture.width / 8, areaTexture.height / 8, 1);
            });

            yield return new WaitUntil(() => isReadbackComplete);
        }

        StartCoroutine(DoReadbackCoroutine());
    }
    
    public static RenderTexture ToRenderTexture(Texture2D texture)
    {
        // Create a RenderTexture with the same dimensions as the Texture2D
        RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0);
        
        // Enable random write access if needed (for compute shader usage)
        renderTexture.enableRandomWrite = true;

        // Activate the RenderTexture and perform the copy
        RenderTexture.active = renderTexture;
        Graphics.Blit(texture, renderTexture);

        // Reset the active RenderTexture
        RenderTexture.active = null;

        return renderTexture;
    }

    void LoadCaptured() {
        if (areaTexture != null && !capturing) {
            //RenderTexture rt = ToRenderTexture(tex);
            //rt.enableRandomWrite = true;
            //rt.Create();
            //Texture2D tx = ConvertRenderTextureToTexture2D(rt);
            Texture2D myTexture = new Texture2D(150, 150, TextureFormat.RGB24, false);
            for (int x = 0; x < myTexture.width; x++)
            {
                for (int y = 0; y < myTexture.height; y++)
                {
                    // Set the pixel to yellow
                    myTexture.SetPixel(x, y, Color.yellow);
                }
            }
            myTexture.Apply();
            //https://stackoverflow.com/questions/72375421/reading-data-from-rwtexture2d-in-compute-shader-unity
            RenderTexture myRenderTexture = ToRenderTexture(myTexture);
            myRenderTexture.enableRandomWrite = true;
            //myRenderTexture.format = RenderTextureFormat.ARGB32;
            SandComputeShader.SetTexture(convertTextureToParticlesHandle, "InputTexture", myRenderTexture);
            SandComputeShader.SetVector("PastePosition", new Vector2(0, 500));
            SandComputeShader.SetVector("AreaSize", new Vector2(myRenderTexture.width, myRenderTexture.height));
            SandComputeShader.Dispatch(convertTextureToParticlesHandle, myRenderTexture.width / 8, myRenderTexture.height / 8, 1);

            //SandComputeShader.SetTexture(convertTextureToParticlesHandle, "InputTexture", areaTexture);
            //SandComputeShader.SetVector("PastePosition", new Vector2(0, 500));
            //SandComputeShader.SetVector("AreaSize", new Vector2(areaTexture.width, areaTexture.height));
            //SandComputeShader.Dispatch(convertTextureToParticlesHandle, areaTexture.width / 8, areaTexture.height / 8, 1);
            Debug.print("Load texture?");
        }
        else {
            Debug.print("areaTexture == null || capturing");
        }
    }
    
    private float actionCooldown = 0.1f; // Cooldown time in seconds.
    private float lastActionTime = -1f; // The last time the action was performed.

    void Update() {
        float dt = Time.deltaTime;
        SandComputeShader.SetFloat("dt", dt);
        
        SandComputeShader.Dispatch(transformKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1);

        SandComputeShader.Dispatch(fallSandKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1);
        //SandComputeShader.Dispatch(updateSandKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1); // Dispatch the update kernel after the fall kernel

        if (Input.GetKey(KeyCode.W)) {print("; ");
            particleBuffer.GetData(particleData);
            foreach (Particle p in particleData)
            {
                if (p.type == 2)
                print(p.pressure + "; ");
            }
        } else if (Input.GetKeyUp(KeyCode.C)) {
            DoCapture();
        } else if (Input.GetKeyUp(KeyCode.L)) {
            LoadCaptured();
        } else if (Input.GetKey(KeyCode.D) && Time.time > lastActionTime + actionCooldown) {
            lastActionTime = Time.time;
            particleBuffer.GetData(particleData);
            planetBuffer.GetData(planetData);

            Planet player = planetData[0];

            //if (player.active != 0) {
                int2 offset = new int2(1, 0);
                
                player.pos += offset;
                planetData[0] = player;
                
                Particle[] clonedParticles = (Particle[])particleData.Clone();
                for (int i = 0; i < particleData.Length-1; i++) {
                    //if (clonedParticles[i].stickyToId != 0) continue;
                    Particle cl = clonedParticles[i];
                    cl.position += new float2(1, 0);
                    float newIndexf = cl.position.y * simulationTexture.width + cl.position.x;
                    int newIndex = (int)newIndexf;
                    particleData[newIndex] = cl;
                    
                    
                    //if (i + simulationTexture.width > particleData.Length) continue;
                    //particleData[i + simulationTexture.width] = cl;
                    //particleData[i].stickyToId = -1;
                    //particleData[i].velocity = 0;
                    //particleData[i].type = 0;
                    //particleData[i].color = new Vector4(0,0,0,1);
                }

                /*
                Particle prev = particleData[0];
                bool prevSet = false;
                for (int i = 0; i < particleData.Length; i++) {
                    //(particleData[i], prev) = (prev, particleData[i]);
                    Particle curr = particleData[i];
                    if (curr.stickyToId != 0) continue;

                    curr.position += offset;
    
                    if (!prevSet) {
                        prevSet = true;
                        prev = curr;
                    }
                    else {
                        particleData[i] = prev;
                        prev = curr;
                    }
                }*/
                particleBuffer.SetData(particleData);
                planetBuffer.SetData(planetData);
            //} 
        }

        if (Input.GetMouseButton(0)) {
            Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            uint2 texMousePos = new uint2((uint)(mousePos.x * simulationTexture.width), (uint)(mousePos.y * simulationTexture.height));
            SandComputeShader.SetVector("mousePos", new Vector4(texMousePos.x, texMousePos.y, 0, 0));
            
            SandComputeShader.Dispatch(addSandKernelHandle, simulationTexture.width, simulationTexture.height, 1);
        } else if (Input.GetMouseButtonDown(1)) {
            SandComputeShader.SetInt("candidate", ++candidate);
            Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            uint2 texMousePos = new uint2((uint)(mousePos.x * simulationTexture.width), (uint)(mousePos.y * simulationTexture.height));
            SandComputeShader.SetVector("mousePos", new Vector4(texMousePos.x, texMousePos.y, 0, 0));

            SandComputeShader.Dispatch(addPlanetKernelHandle, simulationTexture.width, simulationTexture.height, 1);
        } else if (Input.GetKeyDown(KeyCode.A) && Time.time > lastActionTime + actionCooldown) {
            lastActionTime = Time.time;
            
        }
    }

    private void OnDestroy() {
        particleBuffer.Release();
        simulationTexture.Release();
        planetBuffer.Release();
        rngBuffer.Release();
        atomicCounter.Release();
    }
}