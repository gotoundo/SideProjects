using UnityEngine;
using System.Collections;

public class UIBountyIcon : MonoBehaviour
{
    Transform worldPosition;
    public Vector3 offset = Vector3.zero;
    bool following = false;

    public void Follow(GameObject worldObjectToFollow)
    {
        transform.SetParent(GameManager.Main.HealthBarFolder.transform);
        worldPosition = worldObjectToFollow.transform;
        following = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (following)
        {
            if (worldPosition == null)
                Destroy(gameObject);
            else
                transform.position = Camera.main.WorldToScreenPoint(worldPosition.position) + offset;
        }
    }
}
