using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public int xSize ,ySize ;

    public GameObject block;


    void Start()
    {
        createGrid();

    }

    private void createGrid()
    {
        for (int x = 0;x<=xSize;x++)
        {
            GameObject borderBottom = Instantiate(block) as GameObject;
            borderBottom.GetComponent<Transform>().position = new Vector3(x-xSize/2, -ySize/2, 0);           
            
            GameObject borderTop = Instantiate(block) as GameObject;
            borderTop.GetComponent<Transform>().position = new Vector3(x-xSize/2, ySize-ySize/2, 0);

        }   
        
        
        for (int y = 0;y<=ySize;y++)
        {
            GameObject borderRight = Instantiate(block) as GameObject;
            borderRight.GetComponent<Transform>().position = new Vector3(-xSize/2, y-(ySize/2), 0);           
            
            GameObject borderLeft = Instantiate(block) as GameObject;
            borderLeft.GetComponent<Transform>().position = new Vector3(xSize-(xSize/2),y-(ySize/2), 0);

        }

    }

    void Update()
    {
        
    }
}
