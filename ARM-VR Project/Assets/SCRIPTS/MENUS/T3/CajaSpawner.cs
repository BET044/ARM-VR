using UnityEngine;

public class CajaSpawner : MonoBehaviour
{
    public GameObject cajaPrefab;
    public float spawnInterval = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnCaja), 0f, spawnInterval);
    }

    void SpawnCaja()
    {
        Instantiate(cajaPrefab, transform.position, Quaternion.identity);
    }
}
