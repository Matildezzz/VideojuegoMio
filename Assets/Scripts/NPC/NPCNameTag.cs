using TMPro;
using UnityEngine;

public class NPCNameTag : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NPC npc;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject nameRoot;

    [Header("Jugador")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Distancia")]
    [SerializeField] private float showDistance = 2.2f;
    [SerializeField] private bool alwaysVisibleForDebug = false;

    [Header("Render")]
    [SerializeField] private string sortingLayerName = "Player";
    [SerializeField] private int sortingOrder = 100;

    private bool isVisible;
    private bool warnedMissingPlayer;
    private bool warnedMissingText;

    private void Awake()
    {
        FindReferences();
        ApplySorting();
        RefreshName();
        Hide();
    }

    private void OnEnable()
    {
        if (AlienNameManager.Instance != null)
        {
            AlienNameManager.Instance.OnNamesChanged += RefreshName;
        }
    }

    private void OnDisable()
    {
        if (AlienNameManager.Instance != null)
        {
            AlienNameManager.Instance.OnNamesChanged -= RefreshName;
        }
    }

    private void Start()
    {
        FindPlayerIfNeeded();
        RefreshName();
    }

    private void Update()
    {
        FindReferences();
        FindPlayerIfNeeded();

        if (alwaysVisibleForDebug)
        {
            Show();
            return;
        }

        if (player == null)
        {
            Hide();

            if (!warnedMissingPlayer)
            {
                warnedMissingPlayer = true;
                Debug.LogWarning("NPCNameTag: no encuentro un objeto con tag Player.");
            }

            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= showDistance)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void FindReferences()
    {
        if (npc == null)
        {
            npc = GetComponent<NPC>();
        }

        if (nameText == null)
        {
            nameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (nameRoot == null && nameText != null)
        {
            if (nameText.transform.parent != null)
            {
                nameRoot = nameText.transform.parent.gameObject;
            }
            else
            {
                nameRoot = nameText.gameObject;
            }
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void ApplySorting()
    {
        if (nameText == null)
        {
            return;
        }

        Renderer renderer = nameText.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }

        nameText.raycastTarget = false;
}

    private void RefreshName()
    {
        if (nameText == null)
        {
            return;
        }

        if (npc != null)
        {
            nameText.text = npc.GetNameForUI();
        }
        else
        {
            nameText.text = gameObject.name;
        }
    }

    private void Show()
    {
        isVisible = true;
        RefreshName();
        ApplySorting();

        if (nameRoot != null)
        {
            nameRoot.SetActive(true);
        }
    }

    private void Hide()
    {
        isVisible = false;

        if (nameRoot != null)
        {
            nameRoot.SetActive(false);
        }
    }
}