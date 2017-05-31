using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

    private bool isBomb;
    private bool isNumber;
    private bool isEmpty;
    private bool isShown;
    private bool isMarkSet;
    private bool isFlagSet;

    public GameObject flagRef;
    public GameObject wrongFlagRef;
    public GameObject questionMarkRef;
    public GameObject mineRef;
    public GameObject[] numbersRef;
    public GameObject groundPiecesRef;
    public GameObject lightRef;

    private GameObject flag;
    private GameObject wrongFlag;
    private GameObject questionMark;
    private GameObject mine;
    private GameObject numbers;
    private GameObject groundPieces;
    private GameObject pointLight;
    
    public int number;
    public int row;
    public int col;
    
    private void Start()
    {
        isBomb = false;
        isNumber = false;
        isEmpty = true;
        isShown = false;
        isMarkSet = false;
        isFlagSet = false;
        number = 0;
    }

    public void SetNumber()
    {
        isEmpty = false;
        isNumber = true;
    }

    public void IncrementNumber()
    {
        number++;
    }

    public void InstantiateLight()
    {
        pointLight = Instantiate(lightRef,
                new Vector3(transform.position.x, 0.9f, transform.position.z),
                Quaternion.identity);
        pointLight.name = "Light";
        pointLight.transform.SetParent(gameObject.transform);
    }

    public void InstantiateNumber(int num = 0)
    {
        if (num > 0)
            number = num;

        if (number > 0)
        {
            numbers = Instantiate(numbersRef[number - 1],
                new Vector3(transform.position.x - 0.15f, 0.9f, transform.position.z - 0.2f),
                Quaternion.Euler(new Vector3(-90, 180, 0)));
            numbers.name = "Number" + number;
            numbers.transform.SetParent(gameObject.transform);
        }
    }

    public bool IsEmpty()
    {
        return isEmpty;
    }

    public bool IsNumber()
    {
        return isNumber;
    }

    public bool IsShown()
    {
        return isShown;
    }

    public void SetBomb()
    {
        isBomb = true;
        isEmpty = false;
        mine = Instantiate(mineRef,
            new Vector3(transform.position.x, 0.9f, transform.position.z),
            Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
        mine.name = "Landmine";
        mine.transform.SetParent(gameObject.transform);
    }
    
    public bool IsBomb()
    {
        return isBomb;
    }
    
    public void SetFlag()
    {
        isFlagSet = true;
        flag = Instantiate(flagRef, 
            new Vector3(transform.position.x, 1.08f, transform.position.z), 
            Quaternion.Euler(new Vector3(-90, 180, 0))) as GameObject;
        flag.name = "Flag";
        flag.transform.SetParent(gameObject.transform);
    }

    public void UnsetFlag()
    {
        isFlagSet = false;
        Destroy(flag);
    }

    public bool IsFlagSet()
    {
        return isFlagSet;
    }

    public void ShowWrongFlag()
    {
        Destroy(flag);
        wrongFlag = Instantiate(wrongFlagRef,
            new Vector3(transform.position.x, 1.08f, transform.position.z),
            Quaternion.Euler(new Vector3(-90,180,0))) as GameObject;
        wrongFlag.name = "Wrong Flag";
        wrongFlag.transform.SetParent(gameObject.transform);
    }

    public void SetQuestionMark()
    {
        isMarkSet = true;
        questionMark = Instantiate(questionMarkRef,
            new Vector3(transform.position.x - 0.15f, 1.05f, transform.position.z),
            Quaternion.Euler(new Vector3(0, 180, 0))) as GameObject;
        questionMark.name = "QuestionMark";
        questionMark.transform.SetParent(gameObject.transform);
    }

    public void UnsetQuestionMark()
    {
        isMarkSet = false;
        Destroy(questionMark);
    }

    public bool IsQuestionMarkSet()
    {
        return isMarkSet;
    }

    public void Explode()
    {
        ShowBlock();
        groundPieces = Instantiate(groundPiecesRef,
            new Vector3(transform.position.x, 0.5f, transform.position.z), 
            Quaternion.identity) as GameObject;
        try
        {
            Destroy(numbers);
        }
        catch(Exception e) { }
        Destroy(mine);
        GameObject.Find("ExplosionSound").gameObject.GetComponent<AudioSource>().Play();
        StartCoroutine(DestroyGroundPieces());
    }

    public void ShowBlock()
    {
        isShown = true;
        if (transform.FindChild("Cover") != null)
            Destroy(transform.FindChild("Cover").gameObject);
        gameObject.layer = LayerMask.GetMask("Default");
    }

    IEnumerator DestroyGroundPieces()
    {
        yield return new WaitForSeconds(1);
        Destroy(groundPieces.transform.FindChild("Explosion").gameObject);
        yield return new WaitForSeconds(6);
        Destroy(groundPieces);
    }
}
