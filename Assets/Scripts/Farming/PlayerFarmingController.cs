using UnityEngine;

public sealed class PlayerFarmingController : MonoBehaviour, ISeedUser, IToolUser
{
    [Header("Deteccion")]
    [SerializeField] private Transform actionPoint;
    [SerializeField] private float actionRadius = 0.4f;
    [SerializeField] private LayerMask farmPlotLayer;
    [SerializeField] private LayerMask rockLayer;

    private PlayerInventory playerInventory;

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

        return false;
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