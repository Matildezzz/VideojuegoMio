using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AlienDiaryUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject diaryRoot;
    [SerializeField] private TMP_Text diaryText;

    [Header("Controles")]
    [SerializeField] private Key toggleKey = Key.J;

    private void Start()
    {
        if (AlienDiaryManager.Instance != null)
        {
            AlienDiaryManager.Instance.OnDiaryChanged += Refresh;
        }

        Close();
        Refresh();
    }

    private void OnDestroy()
    {
        if (AlienDiaryManager.Instance != null)
        {
            AlienDiaryManager.Instance.OnDiaryChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (diaryRoot == null)
        {
            return;
        }

        bool newState = !diaryRoot.activeSelf;
        diaryRoot.SetActive(newState);
        PauseController.SetPause(newState);
        Refresh();
    }

    public void Close()
    {
        if (diaryRoot != null)
        {
            diaryRoot.SetActive(false);
        }
    }

    public void Refresh()
    {
        if (diaryText == null || AlienDiaryManager.Instance == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("REGISTRO DE OBSERVACIONES");
        builder.AppendLine();

        List<AlienDiaryEntry> entries = new List<AlienDiaryEntry>(AlienDiaryManager.Instance.Entries);
        entries.Sort((a, b) => string.Compare(a.category + a.title, b.category + b.title, System.StringComparison.Ordinal));

        bool hasAny = false;
        for (int i = 0; i < entries.Count; i++)
        {
            AlienDiaryEntry entry = entries[i];
            if (entry == null || !entry.unlocked)
            {
                continue;
            }

            hasAny = true;
            builder.AppendLine("[" + entry.category + "] " + entry.title);
            builder.AppendLine(entry.content);
            builder.AppendLine();
        }

        if (!hasAny)
        {
            builder.AppendLine("Todavía no hay observaciones. El planeta sigue siendo sospechoso.");
        }

        diaryText.text = builder.ToString();
    }
}
