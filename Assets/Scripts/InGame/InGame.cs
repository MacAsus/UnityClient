﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGame : MonoBehaviour
{
    public GameObject PingLabel;
    public static GameObject Player;
    public GameObject ChatModule;
    public GameObject ChatInputField;
    public Texture2D defaultMouse;
    public Text PlayerHP;
    public Text PlayerMagazine;
    public static bool keyboardInputDisabled = false;
    public GameObject Map;
    public GameObject OptionModalGameObj;
    public GameObject Boss;

    // Use this for initialization
    void Start()
    {
        // 과녁 모양으로 마우스 세팅
        Cursor.SetCursor(defaultMouse, new Vector2(defaultMouse.width / 2, defaultMouse.height / 2), CursorMode.Auto);
        SpawnCharacter();
        // StartCoroutine("ChangeCharacter");
        // StartCoroutine("ChangeWeapon");
        StartDailogue();


        this.GetComponent<BossHPSystem>().EnableHpSlider();
        this.GetComponent<BossHPSystem>().SetMaxBossHP((int)Util.F_ROBOT_HP);
        this.GetComponent<BossHPSystem>().SetBossText("Robot");

    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<BossHPSystem>().SetBossHP((int)Boss.GetComponent<EnemyGeneral>().n_hp);
        if(Player && Player.GetComponent<Player>()) {
            PlayerHP.text = Player.GetComponent<Player>().n_hp+"";
            PlayerMagazine.text = Player.GetComponent<Player>().cur_Weapon.f_Magazine+" / " + Player.GetComponent<Player>().cur_Weapon.s_GunName;
        }
        PingLabel.GetComponent<Text>().text = "Ping: " + PhotonNetwork.GetPing();

        // If Space Close Dialogue
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindObjectOfType<DialogueManager>().DisplayNextSentence();
        }

        // If Enter Open Chat
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(!keyboardInputDisabled || ChatInputField.GetComponent<InputField>().text == "") {
                keyboardInputDisabled = !keyboardInputDisabled;
                ChatModule.SetActive(keyboardInputDisabled);
                ChatInputField.GetComponent<InputField>().ActivateInputField(); 
            }
        }

        // If Tab Open Map
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenMap();
        }

        // If ESC Open Map
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("오픈 모달!!!");
            OpenOptionModal();
        }
    }

    void SpawnCharacter()
    {
        string job = (string)PhotonNetwork.player.CustomProperties["job"]; // "Attacker" || "Tanker" || "Healer" || "Heavy"
        Debug.Log("Job is " + job);
        Player = PhotonNetwork.Instantiate("Character/" + job, new Vector3(0f, -4.0f, 0f), Quaternion.identity, 0);
    }

    IEnumerator ChangeWeapon() {
        WWW www = new WWW("https://fallen-angel.s3.amazonaws.com/yoohoogun114%40naver.com_attacker.png");
        yield return www;
        Player.transform.Find("Weapon").GetComponent<Renderer>().material.mainTexture = www.texture;
    }

    IEnumerator ChangeCharacter() {
        WWW www = new WWW("https://fallen-angel.s3.amazonaws.com/yoohoogun114%40naver.com_attacker.png");
        yield return www;
        System.IO.File.WriteAllBytes(Application.dataPath + "/../Assets/Sprites/SpriteSheet/Attacker/Idle_Front.png", www.bytes);
    }

    void StartDailogue()
    {
        string name = "Hi";
        string[] sentences = { "Hello My Name is Fallen Angel", "It Is Awesome Game And Playful!" };

        Dialogue newDialogue = new Dialogue("player", sentences);
        DialogueTrigger.TriggerDialogue(newDialogue);
    }

    void OpenMap() {
        if(!Map.activeInHierarchy) {
            Map.SetActive(true);
        } else {
            Map.SetActive(false);
        }
    }

    void OpenOptionModal() {
        if(!OptionModalGameObj.activeInHierarchy) {
            this.GetComponent<OptionModal>().Init();
            OptionModalGameObj.SetActive(true);
            keyboardInputDisabled = true;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
            OptionModal.IsActive = true;
        } else {
            OptionModalGameObj.SetActive(false);
            keyboardInputDisabled = false;
            Cursor.SetCursor(defaultMouse, new Vector2(defaultMouse.width / 2, defaultMouse.height / 2), CursorMode.Auto);
            OptionModal.IsActive = false;
        }
    }
}