using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{

    private static Collider[] hits = new Collider[32];
    private static List<IDamageable> targets = new List<IDamageable>();

    /// <summary>
    /// Spawns an explosion particle effect, as well as applying force to all damageable objects caught in the radius.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="radius"></param>
    /// <param name="power"></param>
    /// <param name="explosionLayers"></param>
    public static void ExplosionForce(Vector3 centre, float radius, float power, LayerMask explosionLayers)
    {
        int count = Physics.OverlapSphereNonAlloc(centre, radius, hits, explosionLayers);

        DamageInfo dmgInfo = new DamageInfo();
        dmgInfo.forceMode = ForceMode.VelocityChange;
        dmgInfo.damage = power;

        EffectsManager.SpawnExplosion(centre);

        targets.Clear();
        for (int i = 0; i < count; i++)
        {
            IDamageable pTarget = hits[i].transform.root.GetComponent<IDamageable>();

            if (pTarget == null)
            {
                continue;
            }
            if (targets.Contains(pTarget))
            {
                continue;
            }

            Vector3 dir = (pTarget.GetPosition() - centre).normalized;
            Vector3 forceDir = (pTarget.GetPosition() - centre + Vector3.up).normalized * power;
            dmgInfo.force = forceDir;
            pTarget.TakeDamage(dmgInfo);
            targets.Add(pTarget);
        }


    }
}
