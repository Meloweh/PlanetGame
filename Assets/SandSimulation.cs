using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class SandSimulation : MonoBehaviour {
    public ComputeShader SandComputeShader;
    private RenderTexture simulationTexture;
    
    private int addSandKernelHandle;
    private int fallSandKernelHandle;
    private int transformKernelHandle;
    private int addPlanetKernelHandle;

    public RawImage rawImageDisplay;
    public Canvas canvas;
    struct Particle {
        public Vector4 color;
        public float2 velocity;
        public uint type;
        public int stickyToId;
        public uint mass;
        public float2 position;
    }
    
    struct Planet {
        public uint id;
        public float2 pos;
        public uint mass;
        public uint active;
    }

    private ComputeBuffer particleBuffer, planetBuffer, rngBuffer, atomicCounter;

    private Particle[] particleData;
    private Planet[] planetData;
    
    const int planetCount = 32;
    const int rngCount = 32;
    int candidate = -1;

    //private Random random = new Random();

    //private void spawnPlanet(Vector2 pos) {
        
    //}

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

        rngBuffer = new ComputeBuffer(rngCount, sizeof(uint));

        particleBuffer = new ComputeBuffer(simulationTexture.width * simulationTexture.height, 44);
        particleData = new Particle[simulationTexture.width * simulationTexture.height];
        for (int i = 0; i < particleData.Length; i++) {
            particleData[i].color = new Vector4(0, 0, 0, 1);
            particleData[i].velocity = Vector2.zero;
            particleData[i].type = 0;
            particleData[i].stickyToId = -1; // Initialize to 0
            particleData[i].mass = 0;
            particleData[i].position = Vector2.zero;
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
       
        
        SandComputeShader.SetBuffer(fallSandKernelHandle, "planets", planetBuffer);
        SandComputeShader.SetBuffer(addPlanetKernelHandle, "planets", planetBuffer);


        SandComputeShader.SetTexture(transformKernelHandle, "ResultTexture", simulationTexture);
    }
    
    private float actionCooldown = 0.1f; // Cooldown time in seconds.
    private float lastActionTime = -1f; // The last time the action was performed.

    void Update() {
        float dt = Time.deltaTime;
        SandComputeShader.SetFloat("dt", dt);
        
        SandComputeShader.Dispatch(transformKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1);

        SandComputeShader.Dispatch(fallSandKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1);
        //SandComputeShader.Dispatch(updateSandKernelHandle, simulationTexture.width / 8, simulationTexture.height / 8, 1); // Dispatch the update kernel after the fall kernel

        if (Input.GetKey(KeyCode.D) && Time.time > lastActionTime + actionCooldown) {
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