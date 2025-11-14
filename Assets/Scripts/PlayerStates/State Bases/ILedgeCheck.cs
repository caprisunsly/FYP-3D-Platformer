using UnityEngine;

public interface ILedgeCheck
{
    bool CheckLedge(PlayerController pc, float height, float distance, float groundHeight)
    {
        //in front of the player refers to the direction they are moving
        Vector3 leftRay = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * (pc.ungatedDir.x + 1);
        Vector3 rightRay = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * (pc.ungatedDir.x - 1);
        Vector3 forwardRay = pc.orientation.transform.forward * pc.ungatedDir.y + pc.orientation.transform.right * pc.ungatedDir.x;

        //if there is no ground in front of the player, there is no ledge to grab, so return
        if (!Physics.Raycast(pc.ledgeDetection.position, leftRay, distance, pc.whatIsGround) && !Physics.Raycast(pc.ledgeDetection.position, rightRay, distance, pc.whatIsGround)) return false;
        //if there is ground a set amount above the first ray, then we arent at the top of the wall, so return
        if (Physics.Raycast(pc.ledgeDetection.position + new Vector3(0, height, 0), leftRay, distance, pc.whatIsGround)) return false;
        //if distance from ground is less than the minimum, we are too close to the ground to ledge grab, so return
        if (Physics.Raycast(pc.transform.position, Vector3.down, groundHeight, pc.whatIsGround) && pc.transform.position.y - pc.jumpedFrom < groundHeight) return false;

        return true;
    }
}
