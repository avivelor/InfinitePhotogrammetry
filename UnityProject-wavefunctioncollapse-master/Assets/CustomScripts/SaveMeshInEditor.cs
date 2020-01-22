#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Usage: Attach to gameobject, assign target gameobject (from where the mesh is taken), Run, Press savekey

public class SaveMeshInEditor : MonoBehaviour {
    public string generatedMeshName = "RoomName";
    public Transform selectedParentGameObject;
    public Transform meshCombineObject;
    public bool bSaveAsParentOnly = true;
    public bool bDoEditorSaveMesh = false;
    public bool bDoEditorCombineMesh = false;
    MeshCombine meshUtility = new MeshCombine();

    void Update () {
        if (bDoEditorSaveMesh) {
            bDoEditorSaveMesh = false;
            SaveAsset ();
        }

        if (bDoEditorCombineMesh) {
            bDoEditorCombineMesh = false;
            CombineMeshes ();
        }
    }

    void SaveAsset () {
        if(!bSaveAsParentOnly)
        {
        for (int i = 0; i < selectedParentGameObject.childCount; i++) {
            Transform selectedGameObject = selectedParentGameObject.GetChild(i);
            var mf = selectedGameObject.GetComponent<MeshFilter> ();
            if (mf) {
                var savePath = "Assets/ScannedMeshesIndividual/" + selectedGameObject.name + ".asset";
                Debug.Log ("Saved Mesh to:" + savePath);
                AssetDatabase.CreateAsset (mf.mesh, savePath);
            }
        }
        Debug.Log("Saved " + selectedParentGameObject.childCount.ToString() + " Meshes");
        }
        else
        {
            Transform selectedGameObject = selectedParentGameObject;
            var mf = selectedGameObject.GetComponent<MeshFilter> ();
            if (mf) {
                var savePath = "Assets/ScannedMeshes/" + generatedMeshName + ".asset";
                Debug.Log ("Saved Mesh to:" + savePath);
                AssetDatabase.CreateAsset (mf.mesh, savePath);
            }
        }
    }

    void CombineMeshes()
    {
        meshUtility.combineMeshes(this.gameObject);
    }
}
#endif