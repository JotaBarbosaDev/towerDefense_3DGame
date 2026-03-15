using ActionGameFramework.Health;
using TowerDefense.Targetting;
using UnityEngine;

public class LayerFilteredTargetter : Targetter
{
    public LayerMask allowedLayers = ~0;

    protected override bool IsTargetableValid(Targetable targetable)
    {
        if (!base.IsTargetableValid(targetable))
        {
            return false;
        }

        int targetLayer = targetable.gameObject.layer;
        return (allowedLayers.value & (1 << targetLayer)) != 0;
    }
}
