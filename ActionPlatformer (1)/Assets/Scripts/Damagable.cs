using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Damagable
{
    void Damage(int amount);
    void AddHealth(int amount);
}
