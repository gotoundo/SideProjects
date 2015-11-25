using UnityEngine;

public class FxRotator : MonoBehaviour {
    // === Unit =======================================================================================================
    public Vector3 vector;
    public float speed = 10f;
    public Transform RotationTransform;
    public bool relativeToWorld = false;

    private void Awake() {
        _transform = transform;
        if (vector == Vector3.zero) {
            Debug.LogError("zero");
        }
    }

    private void Update() {
        if (RotationTransform == null) {
            _transform.Rotate(vector, speed * Time.deltaTime, relativeToWorld ? Space.World : Space.Self);
        } else {
            _transform.RotateAround(RotationTransform.position, RotationTransform.forward, speed * Time.deltaTime);
        }
    }

    // === Private ====================================================================================================
    private Transform _transform;
}