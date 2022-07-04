using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorScript : MonoBehaviour
{
    [Header("difficulty increases every section (def=0.1f)")]
    public float DIFFICULTY = 0.1f;
    [Header("distance between neighbour platforms (def=0.8f)")]
    public float platformDistanceModifier = 0.8f;
    [Header("per-section settings (area between full floors)")]
    public float sectionWidth = 10;
    public float sectionHeight = 20;
    public int maxBeansInSection = 2;
    public int maxBoostsInSection = 3;
    [Header("PREFABS")]
    public GameObject wallTile;
    public GameObject floorTile;
    public GameObject genericTopSidedTile;
    public GameObject movingTopSidedTile;
    public GameObject collapsableTopSidedTile;
    public GameObject boosterObject;
    public GameObject bean;

    private int beanPerSec = 0;
    private int boostsInSection = 0;
    private int currentSectionIndex = 0;
    private PlatformerMotor2D playerSettings;
    private Transform playerTransform;
    private float playerJumpHeight = 2;
    private float difficultyModifier = 1.0f;
    private List<GameObject> spawnedSections;
    private GameObject currSection;

    private readonly float playerBoostHeight = 0;
    private readonly float playerSize = 1;
    // Use this for initialization
    void Start()
    {
        spawnedSections = new List<GameObject>();
        playerSettings = FindObjectOfType<PlatformerMotor2D>();
        playerTransform = playerSettings.transform;
        playerJumpHeight = playerSettings.jumpHeight* platformDistanceModifier;
        currentSectionIndex = 0;
        MakeSecion();
    }

    void MakeSecion()
    {
        currSection = new GameObject();
        currSection.name = "Section: " + currentSectionIndex;
        spawnedSections.Add(currSection);
        beanPerSec = 0;
        boostsInSection = 0;
        difficultyModifier = 1.0f + currentSectionIndex* DIFFICULTY;
        MakeWalls();
        MakeFloors();
        MakePlatforms();
        currentSectionIndex++;
    }

    void MakePlatforms()
    {
        float currentPlatformY = currentSectionIndex * sectionHeight;
        float jumpableHeight = currentPlatformY + playerJumpHeight + playerBoostHeight;
        float previousPlatformX = 0.0f;

        do
        {
            float nextPlatformY = Random.Range(currentPlatformY + (playerJumpHeight*0.5f) + (difficultyModifier - 1)*playerJumpHeight/2.0f, currentPlatformY + (playerJumpHeight * 0.8f) + (difficultyModifier-1)*playerJumpHeight/2.0f);
            float minPlatformWidth = Mathf.Max(((0.35f) * sectionWidth)/difficultyModifier, (0.05f) * sectionWidth);
            float maxPlatformWidth = Mathf.Max(((0.7f) * sectionWidth)/difficultyModifier, (0.1f) * sectionWidth);
            float nextPlatformWidth = Random.Range(minPlatformWidth, maxPlatformWidth);
            float nextPlatformX = Random.Range(-sectionWidth / 2.0f + nextPlatformWidth / 2.0f, sectionWidth / 2.0f - nextPlatformWidth / 2.0f);

            currentPlatformY = nextPlatformY;
            GameObject platform = null;

            float platformPickingNumber = Random.Range(0.0f, difficultyModifier);
            float addedDifficulty = difficultyModifier - 1.0f;
            if (platformPickingNumber > 1.0f && platformPickingNumber <= (1.0f + 0.5f * addedDifficulty)){
                platform = Instantiate(collapsableTopSidedTile, new Vector3(nextPlatformX, nextPlatformY), Quaternion.identity, currSection.transform) as GameObject;
            }
            else if (platformPickingNumber > (1.0f + 0.5f * addedDifficulty))
            {
                platform = Instantiate(movingTopSidedTile, new Vector3(nextPlatformX, nextPlatformY), Quaternion.identity, currSection.transform) as GameObject;
            }
            else{
                platform = Instantiate(genericTopSidedTile, new Vector3(nextPlatformX, nextPlatformY), Quaternion.identity, currSection.transform) as GameObject;
            }

            Vector2 initialSize = platform.GetComponent<SpriteRenderer>().size;
            platform.GetComponent<SpriteRenderer>().size = new Vector2(nextPlatformWidth, initialSize.y);

            jumpableHeight = currentPlatformY + playerJumpHeight + playerBoostHeight;

            //Bean handling
            if(beanPerSec < maxBeansInSection && bean != null && Random.Range(0.0f, 2.0f)>=1.5f)
            {
                GameObject beanPickup = null;

                float nextBeanX = Random.Range(-sectionWidth / 2.1f, sectionWidth / 2.1f);
                float nextBeanY = currentPlatformY + 1.0f;

                if (nextBeanX > ((sectionWidth / 2) - 3.0f)) { nextBeanX = ((sectionWidth / 2) - 1.5f - Random.Range(1.0f, 3.0f)); }
                else if (nextBeanX < ((-sectionWidth / 2) + 3.0f)) { nextBeanX = ((-sectionWidth / 2) + 1.5f + Random.Range(1.0f, 3.0f)); }

                beanPickup = Instantiate(bean, new Vector3(nextBeanX, nextBeanY), Quaternion.identity, currSection.transform) as GameObject;
                beanPerSec += 1;
            }

            //Booster handling
            if (currentSectionIndex > 1)
            {
                if (Random.Range(0.0f, 5.0f) < 1.0f && boostsInSection < maxBoostsInSection)
                {
                    Quaternion spawnQuat = Quaternion.identity;
                    float downwards = Random.Range(0.0f, 3.0f);
                    if (downwards > 2.0f)
                    {
                        spawnQuat = Quaternion.Euler(0, 0, 180f);
                    }

                    GameObject boosterInstance = Instantiate(boosterObject, new Vector3(Random.Range(-sectionWidth / 2.0f + nextPlatformWidth / 2.0f, sectionWidth / 2.0f - nextPlatformWidth / 2.0f), nextPlatformY, 0), spawnQuat, currSection.transform);
                    float boosterWidth = Random.Range(1, 3);
                    float boosterHeight = Random.Range(2.0f, 5.0f);

                    boosterInstance.GetComponent<SpriteRenderer>().size = new Vector2(boosterWidth, boosterHeight);
                    boostsInSection++;
                }
            }
        } while ((currentPlatformY + 0.8*playerJumpHeight) <= ((currentSectionIndex+1) * sectionHeight));
    }

    void MakeFloors()
    {
        float currentHeight = currentSectionIndex * sectionHeight;

        GameObject floor;
        if (currentSectionIndex == 0)
        {
            floor = Instantiate(wallTile, Vector3.zero, Quaternion.identity, currSection.transform) as GameObject;
        }
        else
        {
            floor = Instantiate(floorTile, Vector3.zero, Quaternion.identity, currSection.transform) as GameObject;
        }
        Vector2 initalSize = floor.GetComponent<SpriteRenderer>().size;
        floor.transform.position = new Vector3(0, -initalSize.y / 2 + currentHeight, 0);
        floor.GetComponent<SpriteRenderer>().size = new Vector2(sectionWidth + 2 * initalSize.x, initalSize.y);
    }

    void MakeWalls()
    {
        float currentHeight = (sectionHeight / 2.0f) + currentSectionIndex * sectionHeight;

        GameObject leftWall = Instantiate(wallTile, Vector3.zero, Quaternion.identity, currSection.transform) as GameObject;
        GameObject rightWall = Instantiate(wallTile, Vector3.zero, Quaternion.identity, currSection.transform) as GameObject;
        Vector2 initalSize = leftWall.GetComponent<SpriteRenderer>().size;
        leftWall.transform.position = new Vector3((-sectionWidth / 2.0f) - initalSize.x / 2, currentHeight, 0);
        rightWall.transform.position = new Vector3((sectionWidth / 2.0f) + initalSize.x / 2, currentHeight, 0);
        leftWall.GetComponent<SpriteRenderer>().size = new Vector2(initalSize.x, sectionHeight);
        rightWall.GetComponent<SpriteRenderer>().size = new Vector2(initalSize.x, sectionHeight);
    }

    void CheckIfNeedToSpawnNewLevel()
    {
        if (currentSectionIndex <= 0)
            return;
        float checkHeight = (currentSectionIndex - 1) * sectionHeight + 0.5f * sectionHeight;
        if (playerTransform.position.y > checkHeight)
        {
            MakeSecion();
        }
        if (spawnedSections.Count >= 3)
        {
            GameObject temp = spawnedSections[0];
            spawnedSections.RemoveAt(0);
            Destroy(temp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfNeedToSpawnNewLevel();
    }
}
