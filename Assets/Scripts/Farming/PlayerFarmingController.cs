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
        FarmPlot plot = GetTargetPlot();

        if (plot == null)
        {
            return false;
        }

        return plot.TryPlant(seed);
    }

    public bool TryUseTool(ToolItemData tool)
    {
        if (tool == null)
        {
            return false;
        }

        FarmPlot plot = GetTargetPlot();

        if (tool.ToolType == ToolType.Hoe)
        {
            if (plot == null)
            {
                return false;
            }

            return plot.TryTill();
        }

        if (tool.ToolType == ToolType.WateringCan)
        {
            if (plot == null)
            {
                return false;
            }

            return plot.TryWater();
        }

        if (tool.ToolType == ToolType.Pickaxe)
        {
            MineableRock rock = GetTargetRock();

            if (rock == null)
            {
                return false;
            }

            Vector3 minerPosition = actionPoint != null ? actionPoint.position : transform.position;
            return rock.TryMine(playerInventory, minerPosition);
        }

        if (tool.ToolType == ToolType.Weapon)
        {
            if (Time.time < nextWeaponUseTime)
            {
                return false;
            }

            nextWeaponUseTime = Time.time + tool.WeaponCooldown;
            PerformWeaponAttack(tool);

            // Devuelve true aunque no golpee a nadie para que la animación de ataque sí se reproduzca.
            return true;
        }

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

        return hit.GetComponent<FarmPlot>();
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

    private void OnDrawGizmosSelected()
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, actionRadius);
    }
}