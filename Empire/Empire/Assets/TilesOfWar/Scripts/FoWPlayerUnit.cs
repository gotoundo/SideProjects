using UnityEngine;

/// <summary>
/// Should be attached to all "player unit" GameObjects.
/// Allows FoWManager to track the unit's position and destruction.
/// </summary>
public class FoWPlayerUnit : MonoBehaviour
{
    public byte ViewRadiusInTiles = 5;  // the number of tiles away from the unit that should be made "visible"

    [HideInInspector]
    public short PreviousTileX = -1;    // used to know when we move (to update fog)

    [HideInInspector]
    public short PreviousTileZ = -1;    // used to know when we move (to update fog)

    [HideInInspector]
    public byte PreviousRadiusInTiles = 5;  // used to know when our radius changes (to update fog)

    [HideInInspector]
    public bool HasChanged = false;     // tracks whether our fog "lighting" has changed

    private FoWManager _fow;

    /// <summary>
    /// Standard Unity method.
    /// Adds this game object to the FoWManager "viewer" collection
    /// </summary>
    public void Start()
    {
        _fow = FoWManager.FindInstance();

        AddToViewers();
    }

    /// <summary>
    /// Standard Unity method.
    /// Notifies FowManager that this gameobject is no longer a "viewer"
    /// </summary>
    public void OnDestroy()
    {
        RemoveFromViewers();
    }

    public void OnEnable()
    {
        AddToViewers();
    }

    public void OnDisable()
    {
        RemoveFromViewers();
    }

    public void AddToViewers()
    {
        if (_fow == null)
        {
            _fow = FoWManager.FindInstance();
            if (_fow == null)
            {
                Debug.LogWarning("FoWPlayerUnit: FoW not set!");
                return;
            }
        }

        PreviousTileX = -1;
        PreviousTileZ = -1;
        PreviousRadiusInTiles = 0;

        HasChanged = true;

        _fow.AddViewer(gameObject);
    }

    public void RemoveFromViewers()
    {
        if (_fow == null)
        {
            return;
        }

        _fow.RemoveViewer(gameObject);
    }

    /// <summary>
    /// Automatically enables/disables all renderers under the game object
    /// </summary>
    /// <param name="shouldRender">true to enable renderers, false to disable</param>
    public void SetRender(bool shouldRender)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = shouldRender;
        }
    }
}
