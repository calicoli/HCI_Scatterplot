using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisButtonController : MonoBehaviour
{
    public GameObject touchProcessor;
    public GameObject ballController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnCancelSelection()
    {
        ballController.GetComponent<BallController>().ResetBallColorInSelection();
    }

    public void BtnCancelFiltering()
    {
        ballController.GetComponent<BallController>().ResetBallColorInFiltering();
    }

    public void BtnReset()
    {
        touchProcessor.GetComponent<TouchProcessor>().resetAll();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnNavigateMode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterNavigationMode();
    }

    public void BtnSelectPointMode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterSelectionPMode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnFilter1Mode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterFiltering1Mode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnFilter2Mode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterFiltering2Mode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnSelectTetrahedronMode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterSelectionTMode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnSelectDiamondMode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterSelectionDMode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }

    public void BtnSelectAngleTetraMode()
    {
        touchProcessor.GetComponent<TouchProcessor>().enterSelectionAMode();
        ballController.GetComponent<BallController>().UpdateBallPosition();
    }
}
