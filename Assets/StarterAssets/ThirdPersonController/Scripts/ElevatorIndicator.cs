using UnityEngine;

public class ElevatorIndicator : MonoBehaviour
{
    public Renderer listF3;
    public Renderer listF2;
    public Renderer listF1;
    public Renderer listB1;

    public Material buttonWhiteMat;
    public Material buttonPlasticMat;

    public void SelectF3()
    {
        ResetAll();
        listF3.material = buttonPlasticMat;
    }

    public void SelectF2()
    {
        ResetAll();
        listF2.material = buttonPlasticMat;
    }

    public void SelectF1()
    {
        ResetAll();
        listF1.material = buttonPlasticMat;
    }

    public void SelectB1()
    {
        ResetAll();
        listB1.material = buttonPlasticMat;
    }

    void ResetAll()
    {
        listF3.material = buttonWhiteMat;
        listF2.material = buttonWhiteMat;
        listF1.material = buttonWhiteMat;
        listB1.material = buttonWhiteMat;
    }
}