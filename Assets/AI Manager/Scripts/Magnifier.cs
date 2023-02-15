using UnityEngine;

public class Magnifier : MonoBehaviour
{
    public static Magnifier Instance;
    #region Magnification
    public bool Magnification;
    public Camera mainCamera;
    public GameObject MagnifierGlassPrefab;
    [HideInInspector]
    public GameObject magnificationObj;
    [Range(-1f, 1f)]
    public float magnifierVertcal;
    [Range(-1f, 1f)]
    public float magnifierHorizontal;
    [Range(2, 4f)]
    public float magnificationGlassFar;
    public float magnifireSize = 1.0f;
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (!mainCamera)
            Debug.LogError("Main camera not set kindly pass main camera reffernce");
    }
    private void Start()
    {

        if (Magnification)
        {
            if (MagnifierGlassPrefab != null)
            {
                magnificationObj = (GameObject)Instantiate(MagnifierGlassPrefab);
                magnificationObj.transform.parent = mainCamera.transform;
                magnificationObj.transform.localPosition = new Vector3(magnifierHorizontal, magnifierVertcal, magnificationGlassFar);
                magnificationObj.transform.localRotation = Quaternion.Euler(0, 0, 180);
                magnificationObj.transform.localScale = new Vector3(1f, 1f, 1f);
                magnificationObj.layer = 31;
                magnificationObj.transform.GetChild(0).gameObject.layer = 31;

                Camera cam = magnificationObj.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    cam.cullingMask &= ~((1 << 31));
                    cam.clearFlags = mainCamera.clearFlags;
                    cam.backgroundColor = mainCamera.backgroundColor;
                }
                magnificationObj.SetActive(false);
            }
        }


    }
    public void Enable()
    {
        print("Enabled" + !magnificationObj.activeSelf);
        magnificationObj.SetActive(!magnificationObj.activeSelf);

    }
    public void ModufyMaginfier()
    {
        magnificationObj.transform.localPosition = new Vector3(magnifierHorizontal, magnifierVertcal, magnificationGlassFar);
        magnificationObj.transform.localScale = new Vector3(magnifireSize, magnifireSize, magnifireSize);
    }
}

