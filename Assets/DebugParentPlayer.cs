using UnityEngine;

public class DebugPlayerParent : MonoBehaviour
{
    private void Update()
    {
        if (transform.parent != null)
        {
            Debug.Log("PADRE: " + transform.parent.name + " | POS PADRE: " + transform.parent.position);
        }
        else
        {
            Debug.Log("El Player no tiene padre");
        }
    }
}