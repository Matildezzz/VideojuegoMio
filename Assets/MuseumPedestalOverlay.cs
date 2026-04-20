using UnityEngine;

public class MuseumPedestalOverlay : MonoBehaviour
{
    private static MuseumPedestalOverlay instance;

    private string title;
    private string description;
    private Object currentOwner;
    private bool visible;

    private GUIStyle boxStyle;
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOverlay()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject("MuseumPedestalOverlay");
        DontDestroyOnLoad(overlayObject);
        instance = overlayObject.AddComponent<MuseumPedestalOverlay>();
    }

    public static void Show(string itemTitle, string itemDescription, Object owner)
    {
        if (string.IsNullOrWhiteSpace(itemTitle) && string.IsNullOrWhiteSpace(itemDescription))
        {
            return;
        }

        EnsureInstance();

        instance.title = itemTitle ?? string.Empty;
        instance.description = itemDescription ?? string.Empty;
        instance.currentOwner = owner;
        instance.visible = true;
    }

    public static void Hide(Object owner)
    {
        if (instance == null)
        {
            return;
        }

        if (instance.currentOwner != null && owner != null && instance.currentOwner != owner)
        {
            return;
        }

        instance.visible = false;
        instance.currentOwner = null;
        instance.title = string.Empty;
        instance.description = string.Empty;
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        EnsureStyles();

        float width = Mathf.Min(560f, Screen.width - 40f);
        float height = 120f;
        Rect boxRect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 20f, width, height);
        Rect titleRect = new Rect(boxRect.x + 16f, boxRect.y + 12f, boxRect.width - 32f, 28f);
        Rect descriptionRect = new Rect(boxRect.x + 16f, boxRect.y + 44f, boxRect.width - 32f, boxRect.height - 56f);

        GUI.Box(boxRect, GUIContent.none, boxStyle);
        GUI.Label(titleRect, title, titleStyle);
        GUI.Label(descriptionRect, description, descriptionStyle);
    }

    private void EnsureStyles()
    {
        if (boxStyle != null)
        {
            return;
        }

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 18;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.wordWrap = true;
        boxStyle.padding = new RectOffset(12, 12, 12, 12);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = false;

        descriptionStyle = new GUIStyle(GUI.skin.label);
        descriptionStyle.fontSize = 16;
        descriptionStyle.wordWrap = true;
    }
}