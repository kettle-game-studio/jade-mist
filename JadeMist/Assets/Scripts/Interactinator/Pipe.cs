using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Pipe : MonoBehaviour, Interactinator
{
    public SplineContainer splineContainer;
    public MeshRenderer runeMesh;
    public ParticleSystem sparks;
    public float runesSpeed = 1;
    public GameObject activateThisOnFinish;

    Material runeMaterial;
    bool wasAlreadyEnabled;

    void Start()
    {
        runeMaterial = runeMesh.material;
        runeMaterial.SetFloat("_Length", 0);
    }

    public void Interact(PlayerController player, RaycastHit raycastHitInfo)
    {
        if (wasAlreadyEnabled) return;
        wasAlreadyEnabled = true;
        sparks.Stop();
        StartCoroutine(RunRunes());
    }

    IEnumerator RunRunes()
    {
        var length = splineContainer.Spline.GetLength();
        var current = 0f;
        while (current <= length)
        {
            current += Time.deltaTime * runesSpeed;
            runeMaterial.SetFloat("_Length", current);
            yield return null;
        }
        if (activateThisOnFinish != null && activateThisOnFinish.TryGetComponent<Activatinator>(out var activatinator))
        {
            activatinator.Activate();
        }
    }
}
