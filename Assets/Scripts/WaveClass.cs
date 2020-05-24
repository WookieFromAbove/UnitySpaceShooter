using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveClass
{
    public string waveName;

    public GameObject[] enemyPrefabsInWave; // 0 = ramming, 1 = big gun, 2 = spinning
    public int enemiesPerWave;
    public float enemySpawnRate;

    public GameObject[] powerupPrefabsInWave; // 0 = Ammo, 1 = Tripple Shot, 2 = Shield, 3 = Speed Boost, 4 = Health, 5 = Spin (neg), 6 = Bomb
    public int[] powerupRateTable;
    public int powerupTableTotal;
    public float powerupSpawnRate;

    public void CalculatePowerupTableTotal()
    {
        foreach (int rate in powerupRateTable)
        {
            powerupTableTotal += rate;
        }
    }
}
