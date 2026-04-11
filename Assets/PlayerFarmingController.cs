using UnityEngine;

public sealed class PlayerFarmingController : MonoBehaviour, ISeedUser, IToolUser
{
    [Header("Deteccion")]
    [SerializeField] private Transform actionPoint;
    [SerializeField] private float actionRadius = 0.4f;
    [SerializeField] private LayerMask farmPlotLayer;

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

        if (plot == null)
        {
            return false;
        }

        if (tool.ToolType == ToolType.Hoe)
        {
            return plot.TryTill();
        }

        if (tool.ToolType == ToolType.WateringCan)
        {
            return plot.TryWater();
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

    private void OnDrawGizmosSelected()
    {
        Vector3 center = actionPoint != null ? actionPoint.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, actionRadius);
    }
}