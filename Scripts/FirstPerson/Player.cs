using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Player : MonoBehaviour
{

    private enum ControlMode
    {
        /// <summary>
        /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
        /// </summary>
        Tank,
        /// <summary>
        /// Character freely moves in the chosen direction from the perspective of the camera
        /// </summary>
        Direct
    }

    [SerializeField] private float m_moveSpeed = 3.0f;
    [SerializeField] private float m_turnSpeed = 1.0f;
    [SerializeField] private float m_jumpForce = 4.0f;

    [SerializeField] private Animator m_animator = null;
    [SerializeField] private Rigidbody m_rigidBody = null;

    [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 0.33f;
    private readonly float m_backwardsWalkScale = 0.16f;
    private readonly float m_backwardRunScale = 0.66f;

    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;
    private bool m_jumpInput = false;

    // rigid body�� �浹���� ��� ���� ���� �Ǵ� ���� �ذ�
    private bool jumped = false;

    private bool m_isGrounded;

    private List<Collider> m_collisions = new List<Collider>();

    //Inventory 
    public GameObject inventory;
    public GameObject characterSystem;
    public GameObject craftSystem;
    public GameObject storage;
    public GameObject timer;

    private Inventory craftSystemInventory;
    private CraftSystem cS;
    private Inventory StorageInventory;
    private Inventory mainInventory;
    private Inventory characterSystemInventory;
    private Tooltip toolTip;

    private InputManager inputManagerDatabase;

    public GameObject HPHungerCanvas;

    Text hpText;
    Text hungerText;
    GameObject manualText;
    Image hpImage;
    Image hungerImage;

    float maxHealth = 100;
    float maxHunger = 100;
    public float maxDamage = 1;
    float maxArmor = 0;

    public float currentHealth = 100;
    float currentHunger = 100;
    public float currentDamage = 0;
    float currentArmor = 0;

    public float CraftSpeed = 0;
    public float MoveSpeed = 0;
    public float maxRange = 0;

    int normalSize = 4;
    public bool equipAxe;
    public Text state;
    float startTimer;
    Transform trans;
    public int handlenum = -1;

    public void OnEnable()
    {       
        Inventory.ItemEquip += OnBackpack;
        Inventory.UnEquipItem += UnEquipBackpack;

        Inventory.ItemEquip += OnGearItem;
        Inventory.ItemConsumed += OnConsumeItem;
        Inventory.UnEquipItem += OnUnEquipItem;

        Inventory.ItemEquip += EquipWeapon;
        Inventory.UnEquipItem += UnEquipWeapon;
    }

    public void OnDisable()
    {          
        Inventory.ItemEquip -= OnBackpack;
        Inventory.UnEquipItem -= UnEquipBackpack;

        Inventory.ItemEquip -= OnGearItem;
        Inventory.ItemConsumed -= OnConsumeItem;
        Inventory.UnEquipItem -= OnUnEquipItem;

        Inventory.UnEquipItem -= UnEquipWeapon;
        Inventory.ItemEquip -= EquipWeapon;
    }

    void EquipWeapon(Item item)
    {          
        if (item.itemType == ItemType.Weapon)
        {
            if(item.itemID == 63 || item.itemID == 62 ||item.itemID == 61 ){                            
                // Debug.Log(treeComponent.equipAxe);
                // GetComponent<TreeComponent>().equipAxe = true;
                equipAxe = true;
            }
            //add the weapon if you unequip the weapon
        }
    }

    void UnEquipWeapon(Item item)
    {         
        if (item.itemType == ItemType.Weapon)
        {
            if(item.itemID == 63){
                // GetComponent<TreeComponent>().equipAxe = false;              
                equipAxe = false;
            }
            //delete the weapon if you unequip the weapon
        }
    }

    void OnBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
        {
            for (int i = 0; i < item.itemAttributes.Count; i++)
            {
                if (mainInventory == null)
                    mainInventory = inventory.GetComponent<Inventory>();
                mainInventory.sortItems();
                if (item.itemAttributes[i].attributeName == "Slots")
                    changeInventorySize(item.itemAttributes[i].attributeValue);
            }
        }
    }

    void UnEquipBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
            changeInventorySize(normalSize);
    }

    void changeInventorySize(int size)
    {
        dropTheRestItems(size);

        if (mainInventory == null)
            mainInventory = inventory.GetComponent<Inventory>();
        if (size == 4)
        {
            mainInventory.width = 2;
            mainInventory.height = 2;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        else if (size == 9)
        {
            mainInventory.width = 3;
            mainInventory.height = 3;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        else if (size == 16)
        {
            mainInventory.width = 4;
            mainInventory.height = 4;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }

        else if (size == 25)
        {
            mainInventory.width = 5;
            mainInventory.height = 5;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }

        else if (size == 36)
        {
            mainInventory.width = 6;
            mainInventory.height = 6;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
    }

    void dropTheRestItems(int size)
    {
        if (size < mainInventory.ItemsInInventory.Count)
        {
            for (int i = size; i < mainInventory.ItemsInInventory.Count; i++)
            {
                GameObject dropItem = (GameObject)Instantiate(mainInventory.ItemsInInventory[i].itemModel);
                dropItem.AddComponent<PickUpItem>();
                dropItem.GetComponent<PickUpItem>().item = mainInventory.ItemsInInventory[i];
                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
            }
        }
    }

    void Start()
    {

        #region 플레이어 시작 설정

        float startArea = UnityEngine.Random.Range(0, 4);
        switch (startArea)
        {
            case 0:
                this.transform.position = new Vector3(96.5f, -6.3f, -15f);
                break;

            case 1:
                this.transform.position = new Vector3(85f, -6.3f, 96.5f);

                break;

            case 2:
                this.transform.position = new Vector3(-50.5f, -6.3f, 76f);

                break;

            case 3:
                this.transform.position = new Vector3(-62f, -6.5f, -29f);

                break;
        }
        #endregion

        equipAxe = false;

        if (HPHungerCanvas != null)
        {
            hpText = HPHungerCanvas.transform.GetChild(1).GetChild(0).GetComponent<Text>();

            hungerText = HPHungerCanvas.transform.GetChild(2).GetChild(0).GetComponent<Text>();

            hpImage = HPHungerCanvas.transform.GetChild(1).GetComponent<Image>();
            hungerImage = HPHungerCanvas.transform.GetChild(2).GetComponent<Image>();

            UpdateHPBar();
            UpdateHungerBar();
        }

        if (inputManagerDatabase == null)
            inputManagerDatabase = (InputManager)Resources.Load("InputManager");

        if (craftSystem != null)
            cS = craftSystem.GetComponent<CraftSystem>();

        if (GameObject.FindGameObjectWithTag("Tooltip") != null)
            toolTip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
        if (inventory != null)
            mainInventory = inventory.GetComponent<Inventory>();
        if (characterSystem != null)
            characterSystemInventory = characterSystem.GetComponent<Inventory>();
        if (craftSystem != null)
            craftSystemInventory = craftSystem.GetComponent<Inventory>();
        if (storage != null)
            StorageInventory = storage.GetComponent<Inventory>();
        startTimer=0;
        manualText = GameObject.FindGameObjectWithTag("ManualText");
    }

    void UpdateHPBar()
    {
        hpText.text = (currentHealth + "/" + maxHealth);
        float fillAmount = currentHealth / maxHealth;
        hpImage.fillAmount = fillAmount;
    }

    void UpdateHungerBar()
    {
        hungerText.text = (currentHunger + "/" + maxHunger);
        float fillAmount = currentHunger / maxHunger;
        hungerImage.fillAmount = fillAmount;
    }


    public void OnConsumeItem(Item item)
    {
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
            {
                if ((currentHealth + item.itemAttributes[i].attributeValue) > maxHealth)
                    currentHealth = maxHealth;
                else
                    currentHealth += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "Hunger")
            {
                if ((currentHunger + item.itemAttributes[i].attributeValue) > maxHunger)
                    currentHunger = maxHunger;
                else
                    currentHunger += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "Armor")
            {
                if ((currentArmor + item.itemAttributes[i].attributeValue) > maxArmor)
                    currentArmor = maxArmor;
                else
                    currentArmor += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "Damage")
            {
                if ((currentDamage + item.itemAttributes[i].attributeValue) > maxDamage)
                    currentDamage = maxDamage;
                else
                    currentDamage += item.itemAttributes[i].attributeValue;
            }
        }
        if (HPHungerCanvas != null)
        {
            UpdateHungerBar();
            UpdateHPBar();
        }
    }

    public void OnGearItem(Item item)
    {
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
                maxHealth += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Hunger")
                maxHunger += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                maxArmor += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
            {
                if (maxDamage == 1)
                    maxDamage = 0;
                maxDamage += item.itemAttributes[i].attributeValue;

            }
            if (item.itemAttributes[i].attributeName == "Range")
            {
                maxRange += item.itemAttributes[i].attributeValue;
            }
            if (item.itemAttributes[i].attributeName == "CraftSpeed")
            {
                CraftSpeed += item.itemAttributes[i].attributeValue;
                cS.updateTime();
            }
            if (item.itemAttributes[i].attributeName == "MoveSpeed")
                MoveSpeed += item.itemAttributes[i].attributeValue;
        }
        if (HPHungerCanvas != null)
        {
            UpdateHungerBar();
            UpdateHPBar();
            state.text = "체력: " + maxHealth + "      방어력: " + maxArmor + "      공격력: " + maxDamage + "\n  이동속도: " + (m_moveSpeed + MoveSpeed) + "          제작가속: " + CraftSpeed + "%";
        }
        if (item.itemAttributes[0].attributeName == "Damage")
        {
            if (item.itemID < 53)
                handlenum = item.itemID - 47;
            else if (item.itemID < 58)
                handlenum = item.itemID - 50;
            else if (item.itemID < 63)
                handlenum = item.itemID - 53;
            else if (item.itemID < 68)
                handlenum = item.itemID - 56;
            else if (item.itemID < 130)
                handlenum = item.itemID - 114;
            else if (item.itemID < 135)
                handlenum = item.itemID - 115;
        }
        if (item.itemAttributes[0].attributeName == "Range")
            handlenum = item.itemID - 116;

    }

    public void OnUnEquipItem(Item item)
    {
        for (int i = 0; i < item.itemAttributes.Count; i++)
        {
            if (item.itemAttributes[i].attributeName == "Health")
                maxHealth -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Hunger")
                maxHunger -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                maxArmor -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
            {
                maxDamage -= item.itemAttributes[i].attributeValue;
                if (maxDamage == 0)
                    maxDamage = 1;
                //GameObject.FindGameObjectWithTag("Handle").transform.GetChild(handlenum).gameObject.SetActive(false);
            }
            if (item.itemAttributes[i].attributeName == "Range")
            {
                maxRange -= item.itemAttributes[i].attributeValue;
                //GameObject.FindGameObjectWithTag("Handle").transform.GetChild(handlenum).gameObject.SetActive(false);
            }
            if (item.itemAttributes[i].attributeName == "CraftSpeed")
            {
                CraftSpeed -= item.itemAttributes[i].attributeValue;
                cS.updateTime();
            }
            if (item.itemAttributes[i].attributeName == "MoveSpeed")
                MoveSpeed -= item.itemAttributes[i].attributeValue;
        }
        
        if (HPHungerCanvas != null)
        {
            UpdateHungerBar();
            UpdateHPBar();
            state.text = "체력: " + maxHealth + "      방어력: " + maxArmor + "      공격력: " + maxDamage + "\n  이동속도: " + (m_moveSpeed + MoveSpeed) + "          제작가속: " + CraftSpeed + "%";
        }

        for (int i = 0; i < GameObject.FindGameObjectWithTag("Handle").transform.childCount; i++)
            GameObject.FindGameObjectWithTag("Handle").transform.GetChild(i).gameObject.SetActive(false);
        
    }

    public void pickupAnimation()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            m_animator.SetTrigger("pickup");
        }
    }

    private void Awake()
    {
        if (!m_animator) { gameObject.GetComponent<Animator>(); }
        if (!m_rigidBody) { gameObject.GetComponent<Animator>(); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        }
        else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }

    private void Update()
    {
        playerAttack();
        pickupAnimation();

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space))
        {
            if (inventory.activeSelf)
            {
                Color color = mainInventory.GetComponent<Image>().color;
                color[3] = 0.3f;
                mainInventory.GetComponent<Image>().color = color;
                for (int i = 0; i < mainInventory.transform.GetChild(1).childCount; i++)
                {
                    mainInventory.transform.GetChild(1).GetChild(i).GetComponent<Image>().color = color;
                    if (mainInventory.transform.GetChild(1).GetChild(i).childCount > 0)
                        mainInventory.transform.GetChild(1).GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().color = color;
                }
            }
            if (characterSystem.activeSelf)
                characterSystemInventory.closeInventory();
            if (craftSystem.activeSelf)
                craftSystemInventory.closeInventory();
            if (storage.activeSelf)
            {
                GameObject.FindGameObjectWithTag("Storage").GetComponent<StorageInventory>().showstorage();
                storage.SetActive(false);
            }
            if (toolTip != null)
                toolTip.deactivateTooltip();
            if (timer != null)
                timer.SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Space))
        {
            if (inventory.activeSelf)
            {
                Color color = mainInventory.GetComponent<Image>().color;
                color[3] = 1f;
                mainInventory.GetComponent<Image>().color = color;
                for (int i = 0; i < mainInventory.transform.GetChild(1).childCount; i++)
                {
                    mainInventory.transform.GetChild(1).GetChild(i).GetComponent<Image>().color = color;
                    if (mainInventory.transform.GetChild(1).GetChild(i).childCount > 0)
                        mainInventory.transform.GetChild(1).GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().color = color;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        { 
            if(manualText.activeSelf)
                manualText.SetActive(false);
            else
                manualText.SetActive(true);
        }
        if (!m_jumpInput && Input.GetKey(KeyCode.Space))
        {
            m_jumpInput = true;
        }
        //inventory
        if (Input.GetKeyDown(inputManagerDatabase.CharacterSystemKeyCode))
        {
            if (!characterSystem.activeSelf)
            {
                if (!inventory.activeSelf)
                    mainInventory.openInventory();
                characterSystemInventory.openInventory();
            }
            else
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                if (inventory.activeSelf)
                    mainInventory.closeInventory();
                characterSystemInventory.closeInventory();
            }
        }

        if (Input.GetKeyDown(inputManagerDatabase.InventoryKeyCode))
        {
            if (!inventory.activeSelf)
                mainInventory.openInventory();
            else
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                mainInventory.closeInventory();
            }
        }

        if (Input.GetKeyDown(inputManagerDatabase.StorageKeyCode))
        {
            if (inventory.activeSelf)
                if (toolTip != null)
                    toolTip.deactivateTooltip();
            
        }

        if (Input.GetKeyDown(inputManagerDatabase.CraftSystemKeyCode))
        {
            if (!craftSystem.activeSelf)
            {
                if (!inventory.activeSelf)
                    mainInventory.openInventory();
                craftSystemInventory.openInventory();
                cS.NowSet(0);
                cS.ListWithoutItem();
            }
            else
            {
                if (cS != null)
                    cS.backToInventory();
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                if (inventory.activeSelf)
                    mainInventory.closeInventory();
                craftSystemInventory.closeInventory();
            }
        }

        if (Input.GetKeyDown(inputManagerDatabase.CloseButtonKeyCode))
        {
            if (characterSystem.activeSelf)
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                characterSystemInventory.closeInventory();
            }
            if (inventory.activeSelf)
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                mainInventory.closeInventory();
            }
            if (craftSystem.activeSelf)
            {
                if (cS != null)
                    cS.backToInventory();
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                craftSystemInventory.closeInventory();
            }
            if (storage.activeSelf)
            {
                if (toolTip != null)
                    toolTip.deactivateTooltip();
                StorageInventory.closeInventory();
            }
        }
        // state.text = "체력: " + maxHealth + "      방어력: " + maxArmor + "      공격력: " + maxDamage + "\n  이동속도: " + (m_moveSpeed + MoveSpeed) + "          제작가속: " + CraftSpeed+"%";
        if (Time.time - startTimer > 5)
        {
            currentHunger-=1;
            UpdateHungerBar();
            startTimer = Time.time;
        }
        if (handlenum>0&&!GameObject.FindGameObjectWithTag("Handle").transform.GetChild(handlenum).gameObject.activeSelf)
            GameObject.FindGameObjectWithTag("Handle").transform.GetChild(handlenum).gameObject.SetActive(true);

       
    }

    private void FixedUpdate()
    {
        m_animator.SetBool("Grounded", m_isGrounded);

        switch (m_controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        m_wasGrounded = m_isGrounded;
        m_jumpInput = false;
        // jumped = false;
    }

    private void TankUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        bool walk = Input.GetKey(KeyCode.LeftShift);

        if (v < 0)
        {
            if (walk) { v *= m_backwardsWalkScale; }
            else { v *= m_backwardRunScale; }
        }
        else if (walk)
        {
            v *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
        transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

        m_animator.SetFloat("MoveSpeed", m_currentV);

        JumpingAndLanding();
    }

    private void DirectUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && m_jumpInput && !jumped)
        {
            jumped = true;

            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);            
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
            jumped = false;
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }

    public void playerAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_animator.SetTrigger("attack");
        }
    }


    public void AttackedAndDeath(int code)
    {
        if (currentHealth > 0)
        {
            // 0 : 곰, 1 : 여우, 2 : 좀비
            if (code == 0)
            {
                currentHealth -= 40;
                UpdateHPBar();
                Debug.Log("플레이어가 곰에게 피격 당했습니다.");
                Debug.Log("플레이어의 현재 체력은" + currentHealth + "입니다.");
                m_animator.SetTrigger("Attacked");
            }
            
            else if (code == 1)
            {
                currentHealth -= 25;
                UpdateHPBar();
                Debug.Log("플레이어가 여우에게 피격 당했습니다.");
                Debug.Log("플레이어의 현재 체력은" + currentHealth + "입니다.");
                m_animator.SetTrigger("Attacked");
            }

            else
            {
                currentHealth -= 30;
                UpdateHPBar();
                Debug.Log("플레이어가 좀비에게 피격 당했습니다.");
                Debug.Log("플레이어의 현재 체력은" + currentHealth + "입니다.");
                m_animator.SetTrigger("Attacked");
            }
        }
        else if (currentHealth <= 0)
        {
            Destroy(gameObject, 3);
            Debug.Log("플레이어가 사망했습니다.");
            m_animator.SetTrigger("Death");
            new WaitForSeconds(3.0f);
            FindObjectOfType<GameManager>().EndGame();
        }
    }
}
