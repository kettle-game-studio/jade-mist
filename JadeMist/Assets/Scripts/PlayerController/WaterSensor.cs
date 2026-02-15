using UnityEngine;

public class WaterSensor : MonoBehaviour
{
    public bool InWater { get; private set; } = false;
    bool inWaterBuffer = false;

    void FixedUpdate()
    {
        InWater = inWaterBuffer;
        inWaterBuffer = false;
    }

    void OnTriggerStay(Collider other)
    {
        inWaterBuffer = true;
    }
}
