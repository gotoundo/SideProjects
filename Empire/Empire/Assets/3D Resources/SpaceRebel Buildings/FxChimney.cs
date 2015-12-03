using UnityEngine;

public class FxChimney : MonoBehaviour {
    // === Unity ======================================================================================================
    public GameObject BigChimneyPrefab;
    public GameObject SmallChimneyPrefab;
    public Transform BigChimney;
    public Transform[] SmallChimneys;

    private void Awake() {
        // big
        var chimneyGo = (GameObject)Instantiate(BigChimneyPrefab, BigChimney.position, Quaternion.identity);
        chimneyGo.transform.SetParent(BigChimney);
        // small
        _fireCoreEmitters = new ParticleEmitter[SmallChimneys.Length];
        _glowEmitters = new ParticleEmitter[SmallChimneys.Length];
        _delayTimeRest = new float[SmallChimneys.Length];
        _workingTimeRest = new float[SmallChimneys.Length];
        for (var i = 0; i < SmallChimneys.Length; i++) {
            chimneyGo = (GameObject)Instantiate(SmallChimneyPrefab, SmallChimneys[i].position, Quaternion.identity);
            chimneyGo.transform.SetParent(SmallChimneys[i]);
            var emitters = chimneyGo.GetComponentsInChildren<ParticleEmitter>();
            _fireCoreEmitters[i] = emitters[0];
            _glowEmitters[i] = emitters[1];
            _fireCoreEmitters[i].emit = false;
            _glowEmitters[i].emit = false;
            _delayTimeRest[i] = Random.Range(1f, 3f);
        }
    }

    private void Update() {
        for (var i = 0; i < _fireCoreEmitters.Length; i++) {
            if (_delayTimeRest[i] > 0f) {
                _delayTimeRest[i] -= Time.deltaTime;
                if (_delayTimeRest[i] <= 0f) {
                    _fireCoreEmitters[i].emit = true;
                    _glowEmitters[i].emit = true;
                    _workingTimeRest[i] = Random.Range(1f, 3f);
                }
            } else {
                if (_workingTimeRest[i] > 0f) {
                    _workingTimeRest[i] -= Time.deltaTime;
                } else {
                    _fireCoreEmitters[i].emit = false;
                    _glowEmitters[i].emit = false;
                    _delayTimeRest[i] = Random.Range(2f, 5f);
                }
            }
        }
    }

    // === Private ====================================================================================================
    private ParticleEmitter[] _fireCoreEmitters;
    private ParticleEmitter[] _glowEmitters;
    private float[] _delayTimeRest;
    private float[] _workingTimeRest;
}