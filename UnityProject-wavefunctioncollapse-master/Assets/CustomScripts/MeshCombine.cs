using System;
using System.Collections;
using UnityEngine;

public class MeshCombine
{
    public void combineMeshes(GameObject parent)
    {
        MeshFilter targetFilter = null;

        targetFilter = parent.GetComponent<MeshFilter>();

        if (targetFilter == null)
        {
            targetFilter = parent.AddComponent<MeshFilter>();
        }

        MeshRenderer targetRenderer = null;

        targetRenderer = parent.GetComponent<MeshRenderer>();

        if (targetRenderer == null)
        {
            targetRenderer = parent.AddComponent<MeshRenderer>();
        }

        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int index = 0;

        int matIndex = -1;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue;
            if (meshFilters[i].GetComponent<Renderer>().enabled == false)
            {
                continue;
            }
            else if (matIndex == -1)
            {
                matIndex = i;
            }
            if (meshFilters[i].Equals(targetFilter)) continue;

            
            combine[index].mesh = meshFilters[i].sharedMesh;

            combine[index++].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].GetComponent<Renderer>().enabled = false;
        }

        targetFilter.mesh.CombineMeshes(combine);

        targetFilter.GetComponent<Renderer>().material = meshFilters[matIndex].GetComponent<Renderer>().material;

    }
}

