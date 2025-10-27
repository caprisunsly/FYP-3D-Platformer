using System.Collections.Generic;
using UnityEngine;

public class MoveModuleManager : MonoBehaviour
{
    [SerializeField] List<MoveModule> ctrlModules;
    [SerializeField] List<MoveModule> shiftModules;
    [SerializeField] List<MoveModule> spaceModules;

    public void CtrlStart()
    {
        foreach (MoveModule ctrlModule in ctrlModules) ctrlModule.InputStarted();
    }

    public void CtrlStop()
    {
        foreach (MoveModule ctrlModule in ctrlModules) ctrlModule.InputCancelled();
    }

    public void ShiftStart()
    {
        foreach (MoveModule shiftModule in shiftModules) shiftModule.InputStarted();
    }

    public void ShiftStop()
    {
        foreach (MoveModule shiftModule in shiftModules) shiftModule.InputCancelled();
    }

    public void SpaceStart()
    {
        foreach (MoveModule spaceModule in spaceModules) spaceModule.InputStarted();
    }

    public void SpaceStop()
    {
        foreach (MoveModule spaceModule in spaceModules) spaceModule.InputCancelled();
    }

}
