using Meta.WitAi.TTS.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


public class RayIntraction : MonoBehaviour
{
    [SerializeField] private TTSSpeaker _speaker;
    [SerializeField] private InputActionProperty showButton;
    public XRRayInteractor ray;
    private void Update()
    {
        if (ray.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            Debug.Log(raycastResult.gameObject.name);
            if (showButton.action.WasPressedThisFrame())
            {
                if (raycastResult.gameObject.GetComponent<TMP_Text>() != null)
                {
                    _speaker.SpeakQueued((raycastResult.gameObject.GetComponent<TMP_Text>().text));
                }

            }

        }

    }
}



