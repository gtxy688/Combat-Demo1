using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshHighlighter : MonoBehaviour
{
    [SerializeField] List<SkinnedMeshRenderer> meshsToHighlight;
    [SerializeField] Material originalMaterial;
    [SerializeField] Material highlightMaterial;

    public void HighlightMesh(bool highlight)
    {
        foreach (var mesh in meshsToHighlight)
        {
            mesh.material=highlight?highlightMaterial:originalMaterial;
        }
    }
}
