using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;
using System;

public class Jumper : Agent
{
    public float jumpForce = 2f;

    public GameObject Target;// Assign the prefab in the inspector
    public Transform Spawner;
    private List<GameObject> spawnedObjects = new List<GameObject>(); // List to keep track of spawned instances
    public float minTime = 5f;
    public float maxTime = 10f;
    private float timer;
    private float spawnTime;

    private bool isGrounded;
    private bool jumpRequested = false;

    public override void Initialize()
    {

        // Get the Rigidbody and set constraints
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Lock rotation on the X and Z axes
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        }
    }

    public override void OnEpisodeBegin()
    {
        DestroyAllInstances();
        this.transform.localPosition = new Vector3(0, 0.5f, -7);
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        isGrounded = true;
    }

    public void RewardForWallCollision()
    {
        AddReward(0.5f);
        Debug.Log("Jumper rewarded: Mover hit a wall");
    }

    public void SpawnObject()
    {
        GameObject obj = Instantiate(Target, Spawner.position, Quaternion.identity);
        Mover mover = obj.GetComponent<Mover>();
        if (mover != null)
        {
            mover.jumper = this; // Set the Jumper reference.
        }
        else
        {
            Debug.LogError("Failed to find Mover component on the instantiated Target object.");
        }
        mover.jumper = this; // Pass this Jumper instance to the Mover
        spawnedObjects.Add(obj);
    }

    void ResetTimer()
    {
        spawnTime = Random.Range(minTime, maxTime);
        timer = 0;
    }

    public void DestroyAllInstances()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj); // Destroy the object
        }
        spawnedObjects.Clear(); // Clear the list after destroying all objects
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isGrounded); // Assuming isGrounded is a valid bool, which it should be.
    }


    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnTime)
        {
            SpawnObject();
            ResetTimer();
        }
        // Check for space key down and store the state
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (isGrounded && actionBuffers.DiscreteActions[0] == 1)
        {
            // Execute jump action
            GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            AddReward(-0.05f);
            isGrounded = false;
        }
        else
        {
            AddReward(0.01f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for ground contact
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("target"))
        {
            Debug.Log("Collision!!!");
            AddReward(-1f);
            EndEpisode();
        }

    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = jumpRequested ? 1 : 0;
        // Reset the jump request after reading it
        jumpRequested = false;
    }
}
