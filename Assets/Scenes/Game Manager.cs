using ARMagicBar.Resources.Scripts.PlacementBar;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cubePrefab; // Prefab del cubo que quieres instanciar
    public Transform ground; // Transform del suelo para posicionar los cubos
    public float spawnInterval = 10f; // Intervalo de tiempo entre instancias
    public ARPlacementPlaneMesh arScript;

    private void Start()
    {
        // Inicia el proceso de instanciar cubos
        StartCoroutine(SpawnCubes());
    }

    private IEnumerator SpawnCubes()
    {
        while (true)
        {
            SpawnCube();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCube()
    {
        // Calcula una posición aleatoria en el suelo
        Vector3 groundSize = ground.localScale;
        Vector3 spawnPosition = new Vector3(
            Random.Range(-groundSize.x / 2, groundSize.x / 2),
            ground.position.y + 0.5f, // Asegúrate de que el cubo quede sobre el suelo
            Random.Range(-groundSize.z / 2, groundSize.z / 2)
        );

        // Instancia el cubo
        arScript.InstantiateObjectAtPosition(spawnPosition, Quaternion.identity);
    }
}
