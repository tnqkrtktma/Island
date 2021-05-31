using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftResultSlot : MonoBehaviour
{

    CraftSystem craftSystem;
    public int temp = 0;
    GameObject itemGameObject;
    //Inventory inventory;


    // Use this for initialization
    void Start()
    {
        //inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        craftSystem = transform.parent.GetComponent<CraftSystem>();

        /*
        itemGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/Item") as GameObject);
        itemGameObject.transform.SetParent(this.gameObject.transform);
        itemGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
        itemGameObject.GetComponent<DragItem>().enabled = false;
        itemGameObject.SetActive(false);
        itemGameObject.transform.GetChild(1).GetComponent<Text>().enabled = true;
        Color color = itemGameObject.transform.GetChild(0).GetComponent<Image>().color;
        color[3] = 0.3f;
        itemGameObject.transform.GetChild(0).GetComponent<Image>().color = color;
        itemGameObject.transform.GetChild(1).GetComponent<RectTransform>().localPosition = new Vector2(GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().positionNumberX, GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().positionNumberY);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
        if (craftSystem.checkfinalitem())
        {
            Color color = new Color(1f, 1f, 1f, 1f);
            this.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = color;
        }
        else
        {
            Color color = new Color(1f, 1f, 1f, 0.3f);
            this.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = color;
        }
    }


}
