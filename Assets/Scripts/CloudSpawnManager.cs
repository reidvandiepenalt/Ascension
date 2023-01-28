using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawnManager : MonoBehaviour
{
    [SerializeField] Sprite[] cloudSprites;
    [SerializeField] GameObject cloud;
    [SerializeField] Vector2 topLeft;
    [SerializeField] Vector2 bottomRight;

    [SerializeField] float spawnInterval;
    [SerializeField] float spawnIntervalMod = 0.25f;
    float timer = 0;

    private void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            GameObject newCloud = Instantiate(cloud, transform);
            bool leftSpawn = Random.Range(0, 2) == 0;
            newCloud.transform.position = new Vector3(Random.Range(topLeft.x, bottomRight.x),
                Random.Range(bottomRight.y, topLeft.y), 0);
            CloudScript script = newCloud.GetComponent<CloudScript>();
            script.SetConditions(topLeft.x - 10, bottomRight.x + 10, Random.Range(0.6f, 0.8f) * (leftSpawn ? 1 : -1));
            newCloud.GetComponent<SpriteRenderer>().sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        }
    }

    void FixedUpdate()
    {
        if(timer > 0)
        {
            timer -= Time.fixedDeltaTime;
        }
        else
        {
            timer = spawnInterval * (1 + Random.Range(-spawnIntervalMod, spawnIntervalMod));
            GameObject newCloud = Instantiate(cloud, transform);
            bool leftSpawn = Random.Range(0, 2) == 0;
            newCloud.transform.position = new Vector3(leftSpawn ? topLeft.x : bottomRight.x,
                Random.Range(bottomRight.y, topLeft.y), 65);
            CloudScript script = newCloud.GetComponent<CloudScript>();
            script.SetConditions(topLeft.x - 10, bottomRight.x + 10, Random.Range(0.6f, 0.8f) * (leftSpawn ? 1 : -1));
            newCloud.GetComponent<SpriteRenderer>().sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        }
    }
}
