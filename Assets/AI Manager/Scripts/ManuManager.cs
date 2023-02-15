using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ManuManager : MonoBehaviour
{
    public float spawnDistance = .5f;
    public GameObject menu,magnification;
    public InputActionProperty showButton;
    public Slider verticalSlider, horizontal, size, far;
    public GameObject Ray;
    private Transform head;
    private void Start()
    {
        head = Magnifier.Instance.mainCamera.transform;
        if (!head)
            Debug.LogError("Main camera not set kindly pass main camera reffernce");
    }
    void Update()
    {
        if (showButton.action.WasPressedThisFrame())
        {
          
            menu.SetActive(!menu.activeSelf);
            Ray.SetActive(!Ray.activeSelf);

            menu.transform.position = head.position + new Vector3(head.forward.x, 0, head.forward.z).normalized * spawnDistance;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {

            menu.SetActive(!menu.activeSelf);
            Ray.SetActive(!Ray.activeSelf);

            menu.transform.position = head.position + new Vector3(head.forward.x, 0, head.forward.z).normalized * spawnDistance;
            Magnifier.Instance.Enable();
        }
        menu.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
        menu.transform.forward *= -1;
    }
    public void UpdateMag()
    {
        Magnifier.Instance.magnifierVertcal = verticalSlider.value;
        Magnifier.Instance.magnifierHorizontal = horizontal.value;
        Magnifier.Instance.magnifireSize = size.value;
        Magnifier.Instance.magnificationGlassFar = far.value;
        Magnifier.Instance.ModufyMaginfier();
    }    
    public void EnableDisableMagnifire()
    {
        Magnifier.Instance.Enable();
        magnification.SetActive(true);

    }
    public void CloseCanvas()
    {
        menu.SetActive(false);
        Ray.SetActive(false);

    }
}
