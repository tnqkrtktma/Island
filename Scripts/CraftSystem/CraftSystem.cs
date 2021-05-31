using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftSystem : MonoBehaviour
{

    [SerializeField]
    public int finalSlotPositionX;
    [SerializeField]
    public int finalSlotPositionY;
    [SerializeField]
    public int leftArrowPositionX;
    [SerializeField]
    public int leftArrowPositionY;
    [SerializeField]
    public int rightArrowPositionX;
    [SerializeField]
    public int rightArrowPositionY;
    [SerializeField]
    public int leftArrowRotation;
    [SerializeField]
    public int rightArrowRotation;

    public static int N = 0;
    public static int Now = 0;
    public Image finalSlotImage;
    public Image arrowImage;

    //List<CraftSlot> slots = new List<CraftSlot>();
    public List<Item> itemInCraftSystem = new List<Item>();
    public List<GameObject> itemInCraftSystemGameObject = new List<GameObject>();
    BlueprintDatabase blueprintDatabase;
    ItemDataBaseList itemDataBaseList;
    public List<Item> possibleItems = new List<Item>();
    public List<bool> possibletoCreate = new List<bool>();
    public GameObject itemGameObject;
    int s;
    Player player;

    // Use this for initialization
    void Start()
    {
        blueprintDatabase = (BlueprintDatabase)Resources.Load("BlueprintDatabase");
        itemDataBaseList = (ItemDataBaseList)Resources.Load("ItemDataBase");
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        s = 0;
    }

#if UNITY_EDITOR
    [MenuItem("Master System/Create/Craft System")]
    public static void menuItemCreateInventory()
    {
        GameObject Canvas = null;
        if (GameObject.FindGameObjectWithTag("Canvas") == null)
        {
            GameObject inventory = new GameObject();
            inventory.name = "Inventories";
            Canvas = (GameObject)Instantiate(Resources.Load("Prefabs/Canvas - Inventory") as GameObject);
            Canvas.transform.SetParent(inventory.transform, true);
            GameObject panel = (GameObject)Instantiate(Resources.Load("Prefabs/Panel - CraftSystem2") as GameObject);
            panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            panel.transform.SetParent(Canvas.transform, true);
            GameObject draggingItem = (GameObject)Instantiate(Resources.Load("Prefabs/DraggingItem") as GameObject);
            Instantiate(Resources.Load("Prefabs/EventSystem") as GameObject);
            draggingItem.transform.SetParent(Canvas.transform, true);
            panel.AddComponent<CraftSystem>();
        }
        else
        {
            GameObject panel = (GameObject)Instantiate(Resources.Load("Prefabs/Panel - CraftSystem2") as GameObject);
            panel.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
            panel.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            panel.AddComponent<CraftSystem>();
            DestroyImmediate(GameObject.FindGameObjectWithTag("DraggingItem"));
            GameObject draggingItem = (GameObject)Instantiate(Resources.Load("Prefabs/DraggingItem") as GameObject);
            draggingItem.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);
        }
    }
#endif

    void Update()
    {
        if (Now == 0)
            this.transform.GetChild(0).GetComponent<Text>().text = "맨손 제작창";
        if (Now == 1)
            this.transform.GetChild(0).GetComponent<Text>().text = "재료&도구 제작창";
        if (Now == 2)
            this.transform.GetChild(0).GetComponent<Text>().text = "요리 제작창";
        if (Now == 3)
            this.transform.GetChild(0).GetComponent<Text>().text = "방어구 제작창";
        if (Now == 4)
            this.transform.GetChild(0).GetComponent<Text>().text = "무기 제작창";
        checkfinalitem();
    }
    public void setImages()
    {
        finalSlotImage = transform.GetChild(3).GetComponent<Image>();
        arrowImage = transform.GetChild(4).GetComponent<Image>();

        Image image = transform.GetChild(5).GetComponent<Image>();
        image.sprite = arrowImage.sprite;
        image.color = arrowImage.color;
        image.material = arrowImage.material;
        image.type = arrowImage.type;
        image.fillCenter = arrowImage.fillCenter;
    }
    public float makeingtime()
    {
        return blueprintDatabase.blueprints[N].timeToCraft;
    }

    public void setArrowSettings()
    {
        RectTransform leftRect = transform.GetChild(4).GetComponent<RectTransform>();
        RectTransform rightRect = transform.GetChild(5).GetComponent<RectTransform>();

        leftRect.localPosition = new Vector3(leftArrowPositionX, leftArrowPositionY, 0);
        rightRect.localPosition = new Vector3(rightArrowPositionX, rightArrowPositionY, 0);

        leftRect.eulerAngles = new Vector3(0, 0, leftArrowRotation);
        rightRect.eulerAngles = new Vector3(0, 0, rightArrowRotation);
    }

    public void setPositionFinalSlot()
    {
        RectTransform rect = transform.GetChild(3).GetComponent<RectTransform>();
        rect.localPosition = new Vector3(finalSlotPositionX, finalSlotPositionY, 0);
    }

    public int getSizeX()
    {
        return (int)GetComponent<RectTransform>().sizeDelta.x;
    }

    public int getSizeY()
    {
        return (int)GetComponent<RectTransform>().sizeDelta.y;
    }
    public int getN()
    {
        return N;
    }
    public Text getText()
    {
        return this.transform.GetChild(6).GetComponent<Text>();
    }
    public float gettimeToCraft()
    {
        float t = (blueprintDatabase.blueprints[N].timeToCraft) * (1 - (player.CraftSpeed / 100)); 
        if (t < 0)
            return 0;
        else
            return t;
    }
    public void backToInventory()
    {
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            if (transform.GetChild(1).GetChild(i).childCount > 0 && transform.GetChild(1).GetChild(i).GetChild(0).tag == "Item")
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().addItemToInventory(transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.itemID,
                    transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<ItemOnObject>().item.itemValue);
                Destroy(transform.GetChild(1).GetChild(i).GetChild(0).gameObject);
            }
        }
        destroyItems();
    }
    public bool checkItem()
    {
        return false;
    }
    public void Nplus()
    {
        N++;

        if (N >= blueprintDatabase.blueprints.Count)
            N = 0;
        if (Now != blueprintDatabase.blueprints[N].workingStation)
            Nplus();
    }
    public void Nminus()
    {
        N--;
        if (N < 0)
            N = blueprintDatabase.blueprints.Count - 1;
        if (Now != blueprintDatabase.blueprints[N].workingStation)
            Nminus();
    }


    public void NowSet(int i)
    {
        Now = i;
    }
    public void updateTime()
    {
        float t = (blueprintDatabase.blueprints[N].timeToCraft) * (1 - (player.CraftSpeed / 100));
        transform.GetChild(6).GetComponent<Text>().text = "제작시간: " + t + "초";
    }
    public void ListWithoutItem()
    {

        if (Now!=blueprintDatabase.blueprints[N].workingStation)
        {
            N = -1;
            Nplus();
        }
        int n = N;

        Color color = this.transform.GetComponent<Image>().color;
        color[3] = 0.3f;

        transform.GetChild(6).GetComponent<Text>().text="제작시간: "+ gettimeToCraft() +"초";
        s = 0;
        for (int i = 0; i < blueprintDatabase.blueprints[n].ingredients.Count; i++)
        {
            Transform trans = transform.GetChild(1).GetChild(s);
            Item item = itemDataBaseList.getItemByID(blueprintDatabase.blueprints[n].ingredients[i]);
            
            if (item.maxStack < blueprintDatabase.blueprints[n].amount[i])
            {
                for (int j = 0; j < blueprintDatabase.blueprints[n].amount[i]; j += item.maxStack)
                {
                    Item temp = itemDataBaseList.getItemByID(blueprintDatabase.blueprints[n].ingredients[i]);
                    temp.itemValue = temp.maxStack;
                    if(j+ temp.maxStack> blueprintDatabase.blueprints[n].amount[i])
                        temp.itemValue = blueprintDatabase.blueprints[n].amount[i]-j;
                    trans = transform.GetChild(1).GetChild(s);
                    itemGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/Itemclone") as GameObject);
                    itemGameObject.transform.SetParent(trans);
                    itemGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    itemGameObject.GetComponent<ConsumeItem>().enabled = false;
                    itemGameObject.transform.GetChild(0).GetComponent<Image>().color = color;
                    itemGameObject.GetComponent<ItemOnObject>().item = temp;
                    trans.GetComponent<Image>().color = color;
                    s++;
                }
                
            }
            else 
            {
                itemGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/Itemclone") as GameObject);
                itemGameObject.transform.SetParent(trans);
                itemGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                itemGameObject.GetComponent<ConsumeItem>().enabled = false;
                itemGameObject.transform.GetChild(0).GetComponent<Image>().color = color;
                if (blueprintDatabase.blueprints[n].amount[i] != 0)
                {
                    item.itemValue = blueprintDatabase.blueprints[n].amount[i];
                    trans.GetComponent<Image>().color = color;
                }
                itemGameObject.GetComponent<ItemOnObject>().item = item;
                s++;
            }
        }
        itemGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/Item") as GameObject);
        itemGameObject.transform.SetParent(GameObject.FindGameObjectWithTag("ResultSlot").transform);
        itemGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
        itemGameObject.GetComponent<DragItem>().enabled = false;
        itemGameObject.transform.GetChild(0).GetComponent<Image>().color = color;
        itemGameObject.transform.GetChild(1).GetComponent<RectTransform>().localPosition = new Vector2(GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().positionNumberX, GameObject.FindGameObjectWithTag("MainInventory").GetComponent<Inventory>().positionNumberY);
        itemGameObject.GetComponent<ItemOnObject>().item = blueprintDatabase.blueprints[n].finalItem;
    }


    public bool checkfinalitem()
    {
        int check = 0;
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            if (transform.GetChild(1).GetChild(i).childCount > 0&& transform.GetChild(1).GetChild(i).GetChild(0).tag=="Item")
                check++;
        }
        if (check == s)
            return true;
        return false;
    }
    public void destroyItems()
    {

        GameObject.FindGameObjectWithTag("ResultSlot").transform.GetChild(0).GetComponent<ConsumeItem>().makeFinalItemfalse();
        for (int j = 0; j < GameObject.FindGameObjectWithTag("ResultSlot").transform.childCount; j++)
        {
            GameObject dumy = GameObject.FindGameObjectWithTag("ResultSlot").transform.GetChild(j).gameObject;
            if (dumy != null)
                Destroy(dumy);
        }
        Color color = transform.GetChild(3).GetComponent<Image>().color;
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            if (transform.GetChild(1).GetChild(i).childCount > 0)
            {
                for(int j=0;j< transform.GetChild(1).GetChild(i).childCount;j++)
                {
                    GameObject dumy = transform.GetChild(1).GetChild(i).GetChild(j).gameObject;
                    if (dumy != null)
                        Destroy(dumy);
                }
                if (transform.GetChild(1).GetChild(i).GetComponent<Image>().color != color)
                    transform.GetChild(1).GetChild(i).GetComponent<Image>().color = color;
                
            }
        }

    }
    public void nonDestroy()
    {
        for (int i = 0; i < blueprintDatabase.blueprints[N].amount.Count; i++)
            if (blueprintDatabase.blueprints[N].amount[i]==0)
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().inventory.GetComponent<Inventory>().addItemToInventory(blueprintDatabase.blueprints[N].ingredients[i]);
    }
    public void deleteItems(Item item)
    {
        for (int i = 0; i < blueprintDatabase.blueprints.Count; i++)
        {
            if (blueprintDatabase.blueprints[i].finalItem.Equals(item))
            {
                for (int k = 0; k < blueprintDatabase.blueprints[i].ingredients.Count; k++)
                {
                    for (int z = 0; z < itemInCraftSystem.Count; z++)
                    {
                        if (itemInCraftSystem[z].itemID == blueprintDatabase.blueprints[i].ingredients[k])
                        {
                            if (itemInCraftSystem[z].itemValue == blueprintDatabase.blueprints[i].amount[k])
                            {
                                itemInCraftSystem.RemoveAt(z);
                                Destroy(itemInCraftSystemGameObject[z]);
                                itemInCraftSystemGameObject.RemoveAt(z);
                                ListWithoutItem();
                                break;
                            }
                            else if (itemInCraftSystem[z].itemValue >= blueprintDatabase.blueprints[i].amount[k])
                            {
                                itemInCraftSystem[z].itemValue = itemInCraftSystem[z].itemValue - blueprintDatabase.blueprints[i].amount[k];
                                ListWithoutItem();
                                break;
                            }
                        }
                    }
                }
            }
        }
    }



}
