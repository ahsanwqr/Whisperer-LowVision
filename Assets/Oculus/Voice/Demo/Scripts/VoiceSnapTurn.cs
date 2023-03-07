using UnityEngine;
public class VoiceSnapTurn : MonoBehaviour
{
    public Transform playerRig;
    public void LeftSnapTurn()
    {
        playerRig.transform.rotation = playerRig.transform.rotation * Quaternion.Euler(0f, -45f, 0f);
    }
    public void RightSnapTurn()
    {
        playerRig.transform.rotation = playerRig.transform.rotation * Quaternion.Euler(0f, 45f, 0f);
    }
}
