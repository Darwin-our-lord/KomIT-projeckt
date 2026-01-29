using UnityEngine;

public class ColorZone : MonoBehaviour
{
    [SerializeField] public int answerId; // ID of this answer
    public void SetAnswerId(int id)
    {
        answerId = id;
    }

}
