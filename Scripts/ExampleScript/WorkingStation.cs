using UnityEngine;
using System.Collections;

public class WorkingStation : MonoBehaviour
{

    public KeyCode openInventory;
    public GameObject craftSystem;
    public int distanceToOpenWorkingStation = 3;
    public int nowSet;
    bool showCraftSystem=false;
    Inventory craftInventory;
    public GameObject inventory;
    Inventory mainInventory;
    CraftSystem cS;


    // Use this for initialization
    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("MainInventory");
        if (craftSystem != null)
        {
            craftInventory = craftSystem.GetComponent<Inventory>();
            cS = craftSystem.GetComponent<CraftSystem>();
        }
        if (inventory != null)
            mainInventory = inventory.GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {

        float distance = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);

        if (Input.GetKeyDown(openInventory) && distance <= distanceToOpenWorkingStation)
        {
            showCraftSystem = !showCraftSystem;
            if (!craftSystem.activeSelf)
            {
                if (!inventory.activeSelf)
                    mainInventory.openInventory();
                craftInventory.openInventory();
                cS.NowSet(nowSet);
                cS.ListWithoutItem();
            }
            else
            {

                if (GameObject.FindGameObjectWithTag("Tooltip") != null)
                    GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>().deactivateTooltip();
                cS.backToInventory();
                craftInventory.closeInventory();
                mainInventory.closeInventory();
            }
        }


    }
}
