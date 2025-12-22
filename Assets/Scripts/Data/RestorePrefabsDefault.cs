#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RestorePrefabsDefault : MonoBehaviour
{
    [MenuItem("Tools/Revert Overrides on Selected Prefab")]
    public static void RevertPrefabOverrides()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected. Please select a prefab instance in the hierarchy.");
            return;
        }

        PrefabUtility.RevertPrefabInstance(selectedObject, InteractionMode.UserAction);

    }
}
#endif