using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;
using System;

public class CubeAgentReturn : Agent
{
    public Transform Target;
    public Collider Return;
    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5;
    public int maxStepsPerEpisode = 1000;
    private bool targetCollected = false;

    public override void OnEpisodeBegin()
    {
        // Reset the position and orientation if the agent has fallen
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(3, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        // Move the target to a new random location
        Target.localPosition = RandomTargetPosition();
        targetCollected = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize and add agent position observations
        sensor.AddObservation(NormalizeValue(this.transform.localPosition.x, -5, 5));
        sensor.AddObservation(NormalizeValue(this.transform.localPosition.z, -5, 5));

        // Normalize and add target position observations
        sensor.AddObservation(NormalizeValue(Target.localPosition.x, -5, 5));
        sensor.AddObservation(NormalizeValue(Target.localPosition.z, -5, 5));

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0, rotationMultiplier * actionBuffers.ContinuousActions[1], 0);

        if (this.transform.localPosition.y < 0)
        {
            SetReward(-0.1f);
            EndEpisode();
        }

        if (!targetCollected)
        {
            float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
            // Target reached
            if (distanceToTarget < 1.42f)
            {
                SetReward(1.0f);
                targetCollected = true;
                Target.localPosition = new Vector3(0, -100, 0);
            }
        }
        else
        {
            if (Return.bounds.Contains(transform.position)) // Reached green plane
            {
                SetReward(1.0f);
                targetCollected = false;
                
                Target.localPosition = RandomTargetPosition();


            }
        }



    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    // Helper method to normalize values
    private float NormalizeValue(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    private Vector3 RandomTargetPosition()
    {
        // Implement logic to generate a random position on the red plane
        return new Vector3(Random.Range(1, 9), 0.5f, Random.Range(-4, 4));
    }
}
