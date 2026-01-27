using UnityEngine;

public class ChickenQuest : MonoBehaviour
{
    //on interact, pick dialogue option based on current quest state

    int chickenCount;
    public void ChickenCount(int mod)
    {
        chickenCount += mod;
    }


}
