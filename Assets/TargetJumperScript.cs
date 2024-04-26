using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Target; // Assign the prefab in the inspector
    public float minTime = 1.0f;
    public float maxTime = 5.0f;
    private float timer;
    private float spawnTime;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        //timer += Time.deltaTime;
        //if (timer >= spawnTime)
        //{
        //    SpawnObject();
        //    ResetTimer();
        //}
    }

    public void SpawnObject()
    {
        GameObject obj = Instantiate(Target, transform.position, Quaternion.identity);
        obj.AddComponent<Mover>(); // Add the Mover script to the spawned object
    }

    void ResetTimer()
    {
        spawnTime = Random.Range(minTime, maxTime);
        timer = 0;
    }
}
