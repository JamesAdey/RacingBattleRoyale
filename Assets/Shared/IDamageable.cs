using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{

    void TakeDamage(DamageInfo info);

    Vector3 GetPosition();

}
