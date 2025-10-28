// MR InteractiveBody Starter Code
// Fall 2025. IMDM 327
// Instructor. Myungin Lee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InteractiveBody : MonoBehaviour
{
    public float G = 1f; // Gravity constant https://en.wikipedia.org/wiki/Gravitational_constant
    [SerializeField]
    private GameObject Prefab;
    [SerializeField]
    private GameObject interactivePoint;

    private GameObject[] body;
    BodyProperty[] bp;
    private int numberOfSphere = 200;
    public float fastforwardConst;
    TrailRenderer[] trailRenderer;
    private Vector3 previousInteractivePoint; 
    public float interactiveMass; // how much to interact
    public float maxVelocity;
    public float closeDistance;
    int frameCount;

    struct BodyProperty // why struct?
    {                   // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }


    void Start()
    {
        // init condition
        maxVelocity = 2.5f;
        interactiveMass = 10f;
        closeDistance = 5f; // sqrt value
        fastforwardConst = 0.1f;
        // Just like GO, computer should know how many room for struct is required:
        bp = new BodyProperty[numberOfSphere];
        body = new GameObject[numberOfSphere];
        trailRenderer = new TrailRenderer[numberOfSphere];
        // Loop generating the gameobject and assign initial conditions (type, position, (mass/velocity/acceleration)
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            body[i] = Instantiate(Prefab);
            // https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html
            // initial conditions
            float r = 10f;
            // position is (x,y,z). In this case, I want to plot them on the circle with r
            // ******** Fill in this part ******** // Initialization of the position
            body[i].transform.position = new Vector3(r * Mathf.Sin(i * 2f * Mathf.PI / numberOfSphere),
                                                      r * Mathf.Cos(i * 2f * Mathf.PI / numberOfSphere),
                                                      0 + Random.Range(-10f, 10f));
            // z = 180 to see this happen in front of me. Try something else (randomize) too.

            bp[i].velocity = new Vector3(0, 0, 0); // Try different initial condition
            bp[i].mass = Random.Range(1f, 5f); // Simplified. Try different initial condition
            // body[i].GetComponent<MeshRenderer>().enabled = false;

            // + This is just pretty trails
            trailRenderer[i] = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer[i].time = 2.0f;  // Duration of the trail
            trailRenderer[i].startWidth = 0.01f;  // Width of the trail at the start
            trailRenderer[i].endWidth = 0.005f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer[i].material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color
            Gradient gradient = new Gradient();
            float h = (i / (float)numberOfSphere) % 1f;
            float s = 0.45f;        
            float v = 0.98f;        
            Color c = Color.HSVToRGB(h, s, v); 

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                        new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer[i].colorGradient = gradient;

        }
    }

    void FixedUpdate()
    {
        // Loop for N-body gravity
        // How should we design the loop?
        // initailize 
        for (int i = 0; i < numberOfSphere; i++)
        {
            bp[i].acceleration = Vector3.zero; // important
        }

        // Acceleration (Force)  
        for (int i = 0; i < numberOfSphere; i++)
        {
            for (int j = i + 1; j < numberOfSphere; j++)
            {
                // Vector from i to j body. Make sure which vector you are getting.
                Vector3 distance = body[j].transform.position - body[i].transform.position;
                // Gravity
                Vector3 gravity = CalculateGravity(distance, bp[i].mass, bp[j].mass);
                // Apply Gravity
                // F = ma -> a = F/m
                // Gravity is push and pull with same amount. Force: m1 <-> m2

                // .. only if it is not too close
                if (distance.sqrMagnitude > closeDistance)
                {
                    bp[i].acceleration += gravity / bp[i].mass; // why is this +?
                    bp[j].acceleration -= gravity / bp[j].mass; // why is this -? What decided the direction?                   
                }
                else // apply opposite gravity (push) if too close. 
                { // Hatred is stronger than attraction.
                    bp[i].acceleration -= 3f * gravity / bp[i].mass; // 
                    bp[j].acceleration += 3f *gravity / bp[j].mass; // 
                }

            }
        }
    
        // (Force) Hesitation: randomly hover the space for natural behavior  
        for (int i = 0; i < numberOfSphere; i++)
        {
            float randomScale = 10f;
            if (Random.Range(0f, 1.05f) > 1f)
            {
                bp[i].acceleration += new Vector3(randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f));
            }
        }


        // (Force) Interactive Acceleration : reacts to the actuation of the interactive point
        Vector3 rightHandOffset = new Vector3(100f, 100f, 180f);
        // float actuation = 1f+ (previousInteractivePoint - interactivePoint.transform.position).sqrMagnitude;
        closeDistance = 3f + ((previousInteractivePoint - interactivePoint.transform.position).sqrMagnitude);
        float actuation = 1f;
        if (true) // !!! Which logic will you apply?
        {
            for (int i = 0; i < numberOfSphere; i++)
            {
                Vector3 distance = interactivePoint.transform.position - body[i].transform.position;
                bp[i].acceleration += CalculateGravity(distance, bp[i].mass, interactiveMass) / bp[i].mass * actuation;
            }
            previousInteractivePoint = interactivePoint.transform.position;
            // G = 1f + actuation * 0.01f;
        }

        // Apply acceleration to velocity, to position
        for (int i = 0; i < numberOfSphere; i++)
        {
            // velocity is sigma(Acceleration*time)
            bp[i].velocity += bp[i].acceleration * Time.deltaTime * fastforwardConst;
            // Prevent extra ordinary speed
            body[i].transform.position += bp[i].velocity * Time.deltaTime * fastforwardConst;
            body[i].transform.LookAt(body[i].transform.position + bp[i].velocity);

            // Limit the maximum velocity
            if (bp[i].velocity.magnitude > maxVelocity)
            {
                bp[i].velocity = maxVelocity * bp[i].velocity.normalized;
            }
        }
        frameCount++;
    }


    // Gravity Fuction
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = new Vector3(0f, 0f, 0f); // note this is also Vector3
                                                   // **** Fill in the function below.
        float eps = 0.1f;
        gravity = G * m1 * m2 / (distanceVector.magnitude + eps) * distanceVector.normalized;
        return gravity;
    }
}

