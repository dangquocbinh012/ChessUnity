using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIInputReceiver : InputReceiver
{
    [SerializeField] private UnityEvent onClick;

    public override void OnInputRecieved()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(Input.mousePosition, gameObject, () =>
            {
                if (onClick != null)
                    onClick.Invoke();
            });
        }
    }
}
