using UnityEngine;

[CreateAssetMenu(fileName = "PlatformData", menuName = "Scriptable Objects/PlatformData")]
public class PlatformData : ScriptableObject
{
    public Sprite RainbowPlatformSprite;
    public Sprite[] CrackedPlatformSprites;
    public GameObject[] SpawnPrefabs;
    public GameObject RainbowSpawnPrefabs;
}
