// InteractiveBody Starter Code
// Fall 2025. IMDM 327
// Instructor. Myungin Lee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Boids : MonoBehaviour
{
    public float G = 1f;
    private GameObject[] fishes;
    private GameObject[] sharks;
    BodyProperty[] bpfish;
    BodyProperty[] bpshark;
    public int numberOfFish = 200;
    public int numberOfSharks = 10;
    public float fastforwardConst = 1f;
    private GameObject interactivePoint;
    static float startdistance = 150f;
    public Vector3 fishSchoolPoint = new Vector3(0f, 9f, startdistance); //where the fish want to go
    public Vector3 fishSchoolPointModified;
    [SerializeField] private GameObject righthand;
    public Vector3 interactPoint;// where to interact 
    private Vector3 previousInteractivePoint = new Vector3(0f, 0f, startdistance); 
    public float interactiveMass; // how much to interact
    public float maxVelocity;
    public float closeDistance;
    public float AttackDistance;
    public GameObject sharkModel;
    public GameObject fishModel;
    int frameCount;
    public AudioSource themeMusic;

    struct BodyProperty
    {
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }


    void Start()
    {
        fishSchoolPointModified = fishSchoolPoint;
        // init condition
        maxVelocity = 30f;
        interactiveMass = 30f;
        closeDistance = 16f; // sqrt value
        // interactive point 
        interactivePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        interactivePoint.transform.position = new Vector3(0f, 0f, 0f);
        interactivePoint.GetComponent<Renderer>().material.color = Color.red;
        bpfish = new BodyProperty[numberOfFish];
        fishes = new GameObject[numberOfFish];
        for (int i = 0; i < numberOfFish; i++)
        {
            fishes[i] = Instantiate(fishModel);
            float r = 10f;
            fishes[i].transform.position = new Vector3(r * Mathf.Sin(i * 2f * Mathf.PI / numberOfFish),
                                                      r * Mathf.Cos(i * 2f * Mathf.PI / numberOfFish),
                                                      startdistance + Random.Range(-10f, 10f));

            bpfish[i].velocity = new Vector3(0, 0, 0);
            bpfish[i].mass = Random.Range(1f, 5f);
        }


        bpshark = new BodyProperty[numberOfSharks];
        sharks = new GameObject[numberOfSharks];
        for (int i = 0; i < numberOfSharks; i++)
        {
            sharks[i] = Instantiate(sharkModel);
            float r = 20f;
            sharks[i].transform.position = new Vector3(r * Mathf.Sin(i * 2f * Mathf.PI / numberOfSharks),
                                                      r * Mathf.Cos(i * 2f * Mathf.PI / numberOfSharks),
                                                      startdistance + Random.Range(-10f, 10f));

            bpshark[i].velocity = new Vector3(0, 0, 0);
            bpshark[i].mass = Random.Range(100f, 200f);
        }
    }

    void FixedUpdate()
    {
        Vector3 FishPos = new Vector3();
        Vector3 SharkPos = new Vector3();
        interactivePoint.transform.position = interactPoint;

        for (int i = 0; i < numberOfFish; i++)
        {
            bpfish[i].acceleration = Vector3.zero;
        }

        for (int i = 0; i < numberOfSharks; i++)
        {
            bpshark[i].acceleration = Vector3.zero;
        }

        for (int i = 0; i < numberOfFish; i++)
        {
            for (int j = i + 1; j < numberOfFish; j++)
            {
                Vector3 distance = fishes[j].transform.position - fishes[i].transform.position;
                Vector3 gravity = CalculateGravity(distance, bpfish[i].mass, bpfish[j].mass);
                if (distance.sqrMagnitude > closeDistance)
                {
                    bpfish[i].acceleration += gravity / bpfish[i].mass;
                    bpfish[j].acceleration -= gravity / bpfish[j].mass;
                }
                else // apply opposite gravity (push) if too close. 
                {
                    bpfish[i].acceleration -= 3f * gravity / bpfish[i].mass;
                    bpfish[j].acceleration += 3f * gravity / bpfish[j].mass;
                }

            }
            for (int k = 0; k < numberOfSharks; k++)
            {
                Vector3 distance = sharks[k].transform.position - fishes[i].transform.position;
                if (distance.sqrMagnitude < AttackDistance)
                {
                    Vector3 gravity = CalculateGravity(distance, bpfish[i].mass, bpshark[k].mass);
                    bpfish[i].acceleration -= 100f * gravity / bpfish[i].mass;
                }
            }
            FishPos += fishes[i].transform.position;
        }
        FishPos = FishPos / numberOfFish;

        for (int i = 0; i < numberOfSharks; i++)
        {
            for (int j = i + 1; j < numberOfSharks; j++)
            {
                Vector3 distance = sharks[j].transform.position - sharks[i].transform.position;
                Vector3 gravity = CalculateGravity(distance, bpshark[i].mass, bpshark[j].mass);
                if (distance.sqrMagnitude > closeDistance)
                {
                    bpshark[i].acceleration += gravity / bpshark[i].mass;
                    bpshark[j].acceleration -= gravity / bpshark[j].mass;
                }
                else // apply opposite gravity (push) if too close. 
                {
                    bpshark[i].acceleration -= 3f * gravity / bpshark[i].mass;
                    bpshark[j].acceleration += 3f * gravity / bpshark[j].mass;
                }

            }
            SharkPos += sharks[i].transform.position;
        }
        SharkPos = SharkPos / numberOfSharks;
    
        // (Force) Hesitation: randomly hover the space for natural behavior  
        for (int i = 0; i < numberOfFish; i++)
        {
            float randomScale = 10f;
            if (Random.Range(0f, 1.05f) > 1f)
            {
                bpfish[i].acceleration += new Vector3(randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f));
            }
        }

        for (int i = 0; i < numberOfSharks; i++)
        {
            float randomScale = 10f;
            if (Random.Range(0f, 1.05f) > 1f)
            {
                bpshark[i].acceleration += new Vector3(randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f));
            }
        }


        // (Force) Interactive Acceleration : reacts to the actuation of the interactive point


        Vector3 rightHandOffset = new Vector3(50f, 50f, startdistance);
        interactPoint = righthand.transform.position;
        float actuation = 1f+ (previousInteractivePoint - interactPoint).sqrMagnitude;
        //if (mp.RightHandPinch)
        {
            for (int i = 0; i < numberOfSharks; i++)
            {
                Vector3 distance = interactPoint - sharks[i].transform.position;
                bpshark[i].acceleration += CalculateGravity(distance, bpshark[i].mass, interactiveMass) / bpshark[i].mass * actuation * 10f;
            }
            previousInteractivePoint = interactPoint;
        }

        for (int i = 0; i < numberOfFish; i++)
            {
                Vector3 distance = new Vector3(0f, 9f, 150f) - fishes[i].transform.position;
                bpfish[i].acceleration += CalculateGravity(distance, bpfish[i].mass, interactiveMass) / bpfish[i].mass * 15f;
            }

        // Apply acceleration to velocity, to position
        for (int i = 0; i < numberOfFish; i++)
        {
            // velocity is sigma(Acceleration*time)
            bpfish[i].velocity += bpfish[i].acceleration * Time.deltaTime * fastforwardConst;
            // Prevent extra ordinary speed
            fishes[i].transform.position += bpfish[i].velocity * Time.deltaTime * fastforwardConst;
            fishes[i].transform.LookAt(fishes[i].transform.position + bpfish[i].velocity);

            // Limit the maximum velocity
            if (bpfish[i].velocity.magnitude > maxVelocity)
            {
                bpfish[i].velocity = maxVelocity * bpfish[i].velocity.normalized;
            }
        }

        for (int i = 0; i < numberOfSharks; i++)
        {
            // velocity is sigma(Acceleration*time)
            bpshark[i].velocity += bpshark[i].acceleration * Time.deltaTime * fastforwardConst;
            // Prevent extra ordinary speed
            sharks[i].transform.position += bpshark[i].velocity * Time.deltaTime * fastforwardConst;
            sharks[i].transform.LookAt(sharks[i].transform.position + bpshark[i].velocity);

            // Limit the maximum velocity
            if (bpshark[i].velocity.magnitude > maxVelocity)
            {
                bpshark[i].velocity = maxVelocity * bpshark[i].velocity.normalized;
            }
        }

        frameCount++;

        float agression = Vector3.Distance(FishPos, SharkPos);
        float v = Mathf.InverseLerp(50f, 10f, agression);
        themeMusic.volume = Mathf.Lerp(0f, 1f, v);
    }


    // Gravity Fuction
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = new Vector3(0f, 0f, 0f);
        float eps = 0.1f;
        gravity = G * m1 * m2 / (distanceVector.magnitude + eps) * distanceVector.normalized;
        return gravity;
    }
}

