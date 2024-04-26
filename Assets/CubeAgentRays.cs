using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgentRays : Agent
{
    public Transform Target;
    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5;
    public int maxStepsPerEpisode = 1000;
    public Rigidbody agentRigidbody;  // Attach Rigidbody component in Unity Editor
    new private int StepCount;

    public override void OnEpisodeBegin()
    {
        StepCount = 0;
        // Reset the position and orientation if the agent has fallen
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        // Move the target to a new random location
        Target.localPosition = new Vector3(Random.Range(-4, 4), 0.5f, Random.Range(-4, 4));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize and add agent position observations
        sensor.AddObservation(NormalizeValue(this.transform.localPosition.x, -5, 5));
        sensor.AddObservation(NormalizeValue(this.transform.localPosition.z, -5, 5));

        // Normalize and add target position observations
        sensor.AddObservation(NormalizeValue(Target.localPosition.x, -5, 5));
        sensor.AddObservation(NormalizeValue(Target.localPosition.z, -5, 5));

        // New velocity observations
        sensor.AddObservation(agentRigidbody.velocity.x);
        sensor.AddObservation(agentRigidbody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0, rotationMultiplier * actionBuffers.ContinuousActions[1], 0);

        if (StepCount >= maxStepsPerEpisode)
        {
            SetReward(-1.0f);
            EndEpisode();
        }

        // Rewards    
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        // Target reached
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        // Fell off the platform?
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
        else
        {
            // Negative reward incrementally to encourage faster solution
            AddReward(-0.01f);
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
}
