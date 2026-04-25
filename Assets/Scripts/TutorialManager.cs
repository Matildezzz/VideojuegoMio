using TMPro;
using UnityEngine;

public enum TutorialStep
{
    Move = 0,
    TalkToNpc = 1,
    OpenInventory = 2,
    UseTool = 3,
    PlantSeed = 4,
    SellOrGift = 5,
    Sleep = 6,
    Completed = 7
}

public sealed class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject tutorialRoot;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Progreso")]
    [SerializeField] private bool saveProgress = true;
    [SerializeField] private bool resetProgressOnStart = false;
    [SerializeField] private string playerPrefsKey = "TutorialStep";

    private TutorialStep currentStep = TutorialStep.Move;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (resetProgressOnStart)
        {
            PlayerPrefs.DeleteKey(playerPrefsKey);
        }

        if (saveProgress)
        {
            int savedStep = PlayerPrefs.GetInt(playerPrefsKey, (int)TutorialStep.Move);
            currentStep = (TutorialStep)Mathf.Clamp(savedStep, 0, (int)TutorialStep.Completed);
        }

        ShowCurrentStep();
    }

    public void NotifyPlayerMoved()
    {
        CompleteStepIfCurrent(TutorialStep.Move);
    }

    public void NotifyTalkedToNpc()
    {
        CompleteStepIfCurrent(TutorialStep.TalkToNpc);
    }

    public void NotifyInventoryOpened()
    {
        CompleteStepIfCurrent(TutorialStep.OpenInventory);
    }

    public void NotifyToolUsed()
    {
        CompleteStepIfCurrent(TutorialStep.UseTool);
    }

    public void NotifySeedPlanted()
    {
        CompleteStepIfCurrent(TutorialStep.PlantSeed);
    }

    public void NotifySoldOrGiftedItem()
    {
        CompleteStepIfCurrent(TutorialStep.SellOrGift);
    }

    public void NotifySlept()
    {
        CompleteStepIfCurrent(TutorialStep.Sleep);
    }

    public void ResetTutorial()
    {
        currentStep = TutorialStep.Move;
        PlayerPrefs.DeleteKey(playerPrefsKey);
        ShowCurrentStep();
    }

    private void CompleteStepIfCurrent(TutorialStep completedStep)
    {
        if (currentStep != completedStep)
        {
            return;
        }

        currentStep++;

        if (saveProgress)
        {
            PlayerPrefs.SetInt(playerPrefsKey, (int)currentStep);
            PlayerPrefs.Save();
        }

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (tutorialRoot != null)
        {
            tutorialRoot.SetActive(currentStep != TutorialStep.Completed);
        }

        if (tutorialText == null)
        {
            return;
        }

        switch (currentStep)
        {
            case TutorialStep.Move:
                tutorialText.text = "Objetivo: muévete\nUsa WASD para empezar a explorar.";
                break;

            case TutorialStep.TalkToNpc:
                tutorialText.text = "Objetivo: habla con un NPC\nAcércate a un personaje y pulsa la tecla de interactuar.";
                break;

            case TutorialStep.OpenInventory:
                tutorialText.text = "Objetivo: abre el inventario\nPulsa I para ver tus objetos.";
                break;

            case TutorialStep.UseTool:
                tutorialText.text = "Objetivo: usa una herramienta\nSelecciona una herramienta en la hotbar y pulsa clic derecho.";
                break;

            case TutorialStep.PlantSeed:
                tutorialText.text = "Objetivo: planta una semilla\nUsa una semilla sobre una parcela preparada.";
                break;

            case TutorialStep.SellOrGift:
                tutorialText.text = "Objetivo: vende o regala un objeto\nVende un objeto en una tienda o pulsa G cerca de un NPC para regalar.";
                break;

            case TutorialStep.Sleep:
                tutorialText.text = "Objetivo: duerme\nInteractúa con la cama para pasar al día siguiente.";
                break;

            default:
                tutorialText.text = "";
                break;
        }
    }
}
