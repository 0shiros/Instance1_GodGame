using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonContainerEvent : MonoBehaviour
{
    [SerializeField] GameEventManager gameEventManager;

    public void CallEvent(int pEventID)
    {
        gameEventManager.events[pEventID].gameEvent.SetupEvent(
            Random.Range(gameEventManager.xLocationMinMax.x, gameEventManager.xLocationMinMax.y),
            Random.Range(gameEventManager.yLocationMinMax.x, gameEventManager.yLocationMinMax.y));
    }
}
