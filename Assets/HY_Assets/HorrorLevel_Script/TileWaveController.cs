using System.Collections;
using UnityEngine;

public class TileWaveController : MonoBehaviour
{
    public RumbleTile[] allTiles;

    [Header("Wave Settings")]
    public int tilesPerWave = 6;
    public float markInterval = 0.4f;   // time between marks
    public float finalDelay = 1.0f;      // time before falling

    public float restTime = 2.5f;


    private void Start()
    {
        StartWave();
    }
    public void StartWave()
    {
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        int markedCount = 0;

        while (markedCount < tilesPerWave)
        {
            RumbleTile tile = GetRandomUnmarkedTile();
            if (tile == null) break;

            tile.Mark();
            markedCount++;

            yield return new WaitForSeconds(markInterval);
        }

        // Final reaction time
        yield return new WaitForSeconds(finalDelay);
        Debug.Log("FinalDelay");
        // Drop all marked tiles
        for (int i = 0; i < allTiles.Length; i++)
        {
            if (allTiles[i].IsMarked)
            {
                allTiles[i].Fall();

            }
        }
        yield return new WaitForSeconds(restTime);
        StartWave();
    }

    RumbleTile GetRandomUnmarkedTile()
    {
        int safety = 50;

        while (safety-- > 0)
        {
            RumbleTile t = allTiles[Random.Range(0, allTiles.Length)];
            if (!t.IsMarked)
                return t;
        }
        return null;
    }
}
