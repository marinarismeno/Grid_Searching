using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    public GameObject mainMenu;
    public Canvas GridCanvas;

    public GameObject playMenu;
    public TMP_Text totalCost;
    public GameObject RecyclePool;
    public GameObject gridPanel;
    public GameObject feedbackPanel;

    public GameObject rowPrefab;
    public GameObject elementPrefab;

    public Sprite targetSprite;
    public Sprite heroSprite;


    public TMP_InputField rows_if, columns_if;
    int rowsNo, columnsNo;

    private Vector2Int currentLocation;
    private Vector2Int previousLocation;
    private Vector2Int step;

    private void Start() 
    {
        totalCost.text = "0"; // reset the total cost field
        ShowMenuPanel(true);
        currentLocation = new Vector2Int(0, 0); //hero's start position
    }
    public void ShowMenuPanel(bool b)
    {
        mainMenu.SetActive(b);
        gridPanel.SetActive(!b);
        playMenu.SetActive(!b);
    }

    public void SetDimentions()
    {
        rowsNo = int.Parse(rows_if.text);
        columnsNo = int.Parse(columns_if.text);
        if (rowsNo < 2 || columnsNo < 2) // set dimention restrictions
        {
            StartCoroutine(ShowFeedback("The minimum grid size is 2x2"));
            return;
        }
        ShowMenuPanel(false);
        ChangeCanvasResolution_Zoom_out();
        CreateGrid();

        PlaceSpriteInGrid(heroSprite, currentLocation); // initialize the hero location
       
        StartCoroutine(FindBestRoute());
    }

    private void ChangeCanvasResolution_Zoom_out()
    {
        CanvasScaler myCanvasScaler = GridCanvas.GetComponent<CanvasScaler>();
        myCanvasScaler.referenceResolution = new Vector2(1920, 1080);

        if (rowsNo > 9 || columnsNo > 20 )
        {
            int dif;
            if (rowsNo > 9)
            {
                dif = rowsNo - 9;
            }               
            else
            {
                dif = columnsNo - 30;
            }               

            dif *= 200;

            myCanvasScaler.referenceResolution = new Vector2(myCanvasScaler.referenceResolution.x + dif, myCanvasScaler.referenceResolution.y);
        }           
    }

    public void CreateGrid()
    {
        GameObject rowitem;
        GameObject columnitem;

        ClearGridPanel();

        for (int i = 0; i < rowsNo; i++)
        {
            rowitem = CheckPoolfor("row");

            if(rowitem == null)
                rowitem = Instantiate(rowPrefab, gridPanel.transform);
            else
                rowitem.transform.SetParent(gridPanel.transform); // use from the pool

            rowitem.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString(); // show the row number

            for (int j = 0; j < columnsNo; j++)
            {
                columnitem = CheckPoolfor("element");
                if (columnitem == null)
                    columnitem = Instantiate(elementPrefab, rowitem.transform);
                else
                    columnitem.transform.SetParent(rowitem.transform); // use from the pool

                if(i==0) // at the first element of the column
                    columnitem.transform.GetChild(0) /* the text */.GetComponent<TMP_Text>().text = j.ToString(); // show the column number at the top
                else
                    columnitem.transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            }
        }
        RandomTargetPlacement(targetSprite);
        gridPanel.SetActive(true);       
    }

    private GameObject CheckPoolfor(string tag)
    {
        Transform poolChild;
        for (int i = 0; i < RecyclePool.transform.childCount; i++)
        {
            poolChild = RecyclePool.transform.GetChild(i);
            if (poolChild.tag == tag)
            {
                return poolChild.gameObject; // found the object with the specific tag
            }               
        }
        return null; // nothing found in the pool
    }

    /**
     * recycling the items for object pooling
     */
    private void ClearGridPanel()
    {
        Transform row;
        currentLocation = new Vector2Int(0, 0);

        for (int i = gridPanel.transform.childCount - 1; i >= 0; --i)
        {
            row = gridPanel.transform.GetChild(i);
            ResetRowItems(row.gameObject);              
        }
    }

    private void ResetRowItems(GameObject row)
    {
        Transform element;

        row.transform.SetParent(RecyclePool.transform);

        for (int i = row.transform.childCount-1; i > 0; i--) // i stops at 1 because the first child of a row object is a text
        {
            element = row.transform.GetChild(i);
            element.GetComponent<Image>().sprite = null; // remove the target sprite
            element.SetParent(RecyclePool.transform);
        }
    }

    public void RandomTargetPlacement(Sprite targetSprite)
    {
        Vector2Int targetPosition =  TargetLocator.GenerateTargetGridPosition(new Vector2Int(rowsNo, columnsNo));
        Debug.Log("target position: " + targetPosition);

        PlaceSpriteInGrid(targetSprite, targetPosition); // initialize the target position     
    }

    public void PlaceSpriteInGrid(Sprite sprite, Vector2Int dimentions)
    {
        Transform targetGO = gridPanel.transform.GetChild(dimentions.x).GetChild(dimentions.y + 1); // y+1 because the first child is the text
        targetGO.GetComponent<Image>().sprite = sprite;
    }

    public IEnumerator FindBestRoute()
    {
        yield return new WaitForSeconds(3);
        TargetLocator.Direction direction;
        Vector2Int newLocation = new Vector2Int();
        int step_1 = 1;
        bool found = false;

        while (!found && playMenu.activeInHierarchy)
        {
            direction = TargetLocator.GetDirectionToTarget(currentLocation);
            Debug.Log("the target is: " + direction);
            step = CalculateStep(step, direction);


            switch (direction)
            {
                case (TargetLocator.Direction.Up):
                    newLocation = new Vector2Int(currentLocation.x - step.x, currentLocation.y);
                    break;
                case (TargetLocator.Direction.Down):
                    newLocation = new Vector2Int(currentLocation.x + step.x, currentLocation.y);
                    break;
                case (TargetLocator.Direction.Right):
                    newLocation = new Vector2Int(currentLocation.x, currentLocation.y + step.y);
                    break;
                case (TargetLocator.Direction.Left):
                    newLocation = new Vector2Int(currentLocation.x, currentLocation.y - step.y);
                    break;
                case (TargetLocator.Direction.DownLeft):
                    newLocation = new Vector2Int(currentLocation.x + step.x, currentLocation.y - step.y);
                    break;
                case (TargetLocator.Direction.DownRight):
                    newLocation = new Vector2Int(currentLocation.x + step.x, currentLocation.y + step.y);
                    break;
                case (TargetLocator.Direction.UpLeft):
                    newLocation = new Vector2Int(currentLocation.x - step.x, currentLocation.y - step.y);
                    break;
                case (TargetLocator.Direction.UpRight):
                    newLocation = new Vector2Int(currentLocation.x - step.x, currentLocation.y + step.y);
                    break;
                case (TargetLocator.Direction.OnTarget):
                    newLocation = currentLocation;
                    Debug.Log("found it!!");
                    found = true;
                    break;
            }
            Debug.Log("new location: " + newLocation);
            previousLocation = currentLocation;
            Debug.Log("previous location: " + previousLocation);


            for (int i = 0; i < step_1; i++) // moving the sprite step by step
            {
                PlaceSpriteInGrid(null, currentLocation); // remove from former location
                PlaceSpriteInGrid(heroSprite, newLocation); // set to new location
            }
            currentLocation = newLocation;
            totalCost.text = TargetLocator.Cost.ToString();
            yield return new WaitForSeconds(1);
        }
        if (playMenu.activeSelf)
            StartCoroutine(ShowFeedback("Found it in " + totalCost.text + " steps!!"));
        else
            totalCost.text = "0"; // reset the cost text
    }

    private Vector2Int CalculateStep(Vector2Int step ,TargetLocator.Direction direction)
    {
        if (direction == TargetLocator.Direction.Down || 
            direction == TargetLocator.Direction.DownRight || direction == TargetLocator.Direction.Right)
        { // +
            step.x = (int) Mathf.Floor((rowsNo-1 - currentLocation.x) / 2);
            step.y = (int) Mathf.Floor((columnsNo-1 - currentLocation.y) / 2);
        }
        else if (direction == TargetLocator.Direction.Up || direction == TargetLocator.Direction.UpLeft 
            || direction == TargetLocator.Direction.Left)
        { 
            step.x = (int)Mathf.Floor((currentLocation.x - previousLocation.x) / 2);
            step.y = (int)Mathf.Floor((currentLocation.y - previousLocation.y) / 2);
        }
        else if (direction == TargetLocator.Direction.DownLeft)
        {
            step.x = (int)Mathf.Floor((rowsNo - 1 - currentLocation.x) / 2);
            step.y = (int)Mathf.Floor((currentLocation.y - previousLocation.y) / 2);
        }
        else if (direction == TargetLocator.Direction.UpRight)
        {
            step.x = (int)Mathf.Floor((currentLocation.x - previousLocation.x) / 2);
            step.y = (int)Mathf.Floor((columnsNo - 1 - currentLocation.y) / 2);
        }

        if (step.x == 0)
            step.x = 1;
        if (step.y == 0)
            step.y = 1;

        return step;
    }

    private IEnumerator ShowFeedback(string s)
    {
        feedbackPanel.SetActive(true);
        feedbackPanel.GetComponentInChildren<TMP_Text>().text = s;

        yield return new WaitForSeconds(2);
        feedbackPanel.SetActive(false);
    }

    private IEnumerator WaitSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public void Exit()
    {
        Application.Quit();
    }


}
