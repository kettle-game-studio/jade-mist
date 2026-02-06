using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightShaker : MonoBehaviour
{
    public AnimationCurve lightCurve;
    public float durationSec = 10;
    public float amplitude = 0.5f;

    float timeStart;
    float lightIntensity;
    float y;
    Light _light;

    void Start()
    {
        timeStart = Time.time - Random.Range(0f, durationSec);
        _light = GetComponent<Light>();
        lightIntensity = _light.intensity;
        y = transform.position.y;
    }

    void Update()
    {
        var time = (Time.time - timeStart) / durationSec % 1f;
        _light.intensity = lightCurve.Evaluate(time) * lightIntensity;
        var x = transform.position.x;
        var z = transform.position.z;
        transform.position = new Vector3(x, y + lightCurve.Evaluate(time) * amplitude, z);
    }
}
