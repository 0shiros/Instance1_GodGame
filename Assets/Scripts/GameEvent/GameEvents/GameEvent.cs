using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent : MonoBehaviour
{
    ///<summary>
    ///overriding function must have an id comparison to know if it should setup
    /// </summary>
    public abstract void SetupEvent(int pX, int pY, float pTimer = 0);
}
