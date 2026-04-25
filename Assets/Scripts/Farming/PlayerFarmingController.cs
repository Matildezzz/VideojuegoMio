using UnityEngine;

public sealed class PlayerFarmingController : MonoBehaviour, ISeedUser, IToolUser
{
    [Header("Deteccion")]
    [SerializeField] private Transform actionPoint;
    [SerializeField] private float actionRadius = 0.4f;
    [SerializeField] private LayerMask farmPlotLayer;
    [SerializeField] private LayerMask rockLayer;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerInventory playerInventory;
    private float nextWeaponUseTime = -1f;

    private void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    public bool TryUseSeed(SeedItemData seed)
    {
        if (seed == null)
        {
            ShowError("Selecciona una semilla valida.");
            return false;
        }

        FarmPlot plot = GetTargetPlot();

        if (plot == null)
        {
            ShowError("No hay una parcela delante.");
            return false;
        }

        bool planted = plot.TryPlant(seed);

        if (planted && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifySeedPlanted();
        }

        return planted;
    }

    public bool TryUseTool(ToolItemData tool)
    {
        if (tool == null)
        {
            ShowError("Selecciona una herramienta valida.");
            return false;
        }

        FarmPlot plot = GetTargetPlot();

        if (tool.ToolType == ToolType.Hoe)
        {
            if (plot == null)
            {
                ShowError("No hay una parcela delante para usar la azada.");
                return false;
            }

            bool usedHoe = plot.TryTill();

            if (usedHoe && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyToolUsed();
            }

            return usedHoe;
        }

        if (tool.ToolType == ToolType.WateringCan)
        {
            if (plot == null)
            {
                ShowError("No hay una parcela delante para regar.");
                return false;
            }

            bool usedWateringCan = plot.TryWater();

            if (usedWateringCan && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyToolUsed();
            }

            return usedWateringCan;
        }

        if (tool.ToolType == ToolType.Pickaxe)
        {
            MineableRock rock = GetTargetRock();

            if (rock == null)
            {
                ShowError("No hay una roca delante para picar.");
                return false;
            }

            Vector3 minerPosition = actionPoint != null ? actionPoint.position : transform.position;
            bool usedPickaxe = rock.TryMine(playerInventory, minerPosition);

            if (usedPickaxe && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyToolUsed();
            }

            if (!usedPickaxe)
            {
                ShowError("No puedes picar esta roca ahora.");
            }

            return usedPickaxe;
        }

        if (tool.ToolType == ToolType.Weapon)
        {
            if (Time.time < nextWeaponUseTime)
            {
                ShowWarning("Espera un momento antes de volver a atacar.");
                return false;
            }

            nextWeaponUseTime = Time.time + tool.WeaponCooldown;
            PerformWeaponAttack(tool);

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyToolUsed();
            }

            return true;
        }

        ShowError("Esta herramienta no se puede usar aqui.");
        return false;
    }

    private void PerformWeaponAttack(ToolItemData tool)
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;
        float radius = Mathf.Max(actionRadius, tool.WeaponRange);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        Collider2D closestHit = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D candidate = hits[i];

            if (candidate == null)
            {
                continue;
            }

            IDamageable damageable = GetDamageable(candidate);

            if (damageable == null || !damageable.IsAlive)
            {
                continue;
            }

            float sqrDistance = ((Vector2)candidate.transform.position - (Vector2)center).sqrMagnitude;

            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestHit = candidate;
            }
        }

        if (closestHit == null)
        {
            return;
        }

        IDamageable target = GetDamageable(closestHit);

        if (target == null || !target.IsAlive)
        {
            return;
        }

        Vector2 hitDirection = ((Vector2)(closestHit.transform.position - transform.position)).normalized;

        if (hitDirection.sqrMagnitude < 0.001f)
        {
            hitDirection = Vector2.down;
        }

        target.TakeDamage(tool.WeaponDamage, hitDirection);
    }

    private IDamageable GetDamageable(Collider2D hit)
    {
        if (hit == null)
        {
            return null;
        }

        IDamageable damageable = hit.GetComponent<IDamageable>();

        if (damageable != null)
        {
            return damageable;
        }

        return hit.GetComponentInParent<IDamageable>();
    }

    private FarmPlot GetTargetPlot()
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(center, actionRadius, farmPlotLayer);

        if (hit == null)
        {
            return null;
        }

        FarmPlot plot = hit.GetComponent<FarmPlot>();

        if (plot != null)
        {
            return plot;
        }

        return hit.GetComponentInParent<FarmPlot>();
    }

    private MineableRock GetTargetRock()
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(center, actionRadius, rockLayer);

        if (hit == null)
        {
            return null;
        }

        MineableRock rock = hit.GetComponent<MineableRock>();

        if (rock != null)
        {
            return rock;
        }

        return hit.GetComponentInParent<MineableRock>();
    }

    private void ShowError(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Error, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void ShowWarning(string message)
    {
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast(message, ToastType.Warning, "Error");
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, actionRadius);
    }
}
