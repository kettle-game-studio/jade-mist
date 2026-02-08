using UnityEngine;

public class Observatory : MonoBehaviour
{
    public Transform Sun;
    public Transform Pointer;
    public MeshRenderer Roof;

    Material roofMaterial;

    void Start()
    {
        roofMaterial = Roof.material;
    }

    void Update()
    {
        roofMaterial.SetVector("_SunPosition", Sun.position);
        Pointer.rotation = Quaternion.LookRotation(Sun.position - Pointer.position);
    }
}
