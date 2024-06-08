using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IHealthComponent
{
    public void AddWound(float BleedRate);
    public void InstantKill(Vector3 killForce);

}
