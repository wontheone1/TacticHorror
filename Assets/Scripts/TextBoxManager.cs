using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour
{
    public GameObject textBox;
    public GameObject enemySpeakerPanel;
    public GameObject playerSpeakerPanel;
    public Text enemySpeaker;
    public Text playerSpeaker;
    public Image enemyImage;
    public Image playerImage;
    public Text theText;
    public TextAsset textFile;
    public XmlDocument dialogue = new XmlDocument();
    public XmlDocument eventDialogue = new XmlDocument();
    public string[] textLines;
    List<entry> lines = new List<entry>();
    List<entry> eventLines = new List<entry>();
    private Statemachine statemachine;
    public int currentLine;
    public int endAtLine;
    public bool dialogueDone = false;
    public bool showingEvent = false;
    public bool textBoxActive = true;

    public class entry
    {
        public string speaker;
        public int team;
        public string line;
        public string eventName;

        public entry(string _speaker, int _team, string _line)
        {
            speaker = _speaker;
            team = _team;
            line = _line;
        }

        public entry(string _speaker, int _team, string _line, string _eventName)
        {
            speaker = _speaker;
            team = _team;
            line = _line;
            eventName = _eventName;
        }
    }
    // public playerController player;

    // Use this for initialization
    void Awake()
    {
        
        try
        {
            textBox = GameObject.Find("DialoguePanel");
            enemySpeakerPanel = GameObject.Find("enemySpeakerPanel");
            playerSpeakerPanel = GameObject.Find("playerSpeakerPanel");
            enemySpeaker = GameObject.Find("enemySpeaker").GetComponent<Text>();
            playerSpeaker = GameObject.Find("playerSpeaker").GetComponent<Text>();
            enemyImage = GameObject.Find("enemyImage").GetComponent<Image>();
            playerImage = GameObject.Find("playerImage").GetComponent<Image>();
            theText = GameObject.Find("Dialogue").GetComponent<Text>();
        }
        catch (Exception e)
        {
            textBoxActive = false;
        }
        statemachine = GetComponent<Statemachine>();
    }

    void Start()
    {
        dialogue.Load("Assets/text/Level1_Dialog.xml");
        eventDialogue.Load("Assets/text/Level1_Event_Dialog.xml");
        try
        {
            playerSpeakerPanel.SetActive(false);
            enemySpeakerPanel.SetActive(false);
        }
        catch (Exception){}
        
        foreach (XmlNode node in dialogue.DocumentElement)
        {
            lines.Add(new entry(node.Attributes[0].Value, int.Parse(node.Attributes[1].Value), node.InnerText));
        }
        if (endAtLine == 0)
        {
            endAtLine = lines.Count - 1;
        }
        if (!textBoxActive)
        {
            endAtLine = 0;
            statemachine.StartGame();
        }
    }

    void Update()
    {
        if (!dialogueDone && textBoxActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentLine++;
            }

            if (currentLine > endAtLine)
            {
                textBox.SetActive(false);
                statemachine.StartGame();
                dialogueDone = true;
            }
            else
            {
                if (lines[currentLine].team == 1)
                {
                    try
                    {
                        playerSpeakerPanel.SetActive(true);
                        enemySpeakerPanel.SetActive(false);
                        playerSpeaker.text = lines[currentLine].speaker;
                        playerImage.sprite = ImageResourcesManager.getInstance().ReturnSprite(lines[currentLine].speaker);
                    }
                    catch (Exception e) { Debug.Log(e); }
                    
                }
                else if (lines[currentLine].team == 2)
                {
                    try
                    {
                        playerSpeakerPanel.SetActive(false);
                        enemySpeakerPanel.SetActive(true);
                        enemySpeaker.text = lines[currentLine].speaker;
                        enemyImage.sprite = ImageResourcesManager.getInstance().ReturnSprite(lines[currentLine].speaker);
                    }
                    catch (Exception e) { }
                }
                theText.text = lines[currentLine].line;
            }
        }

        if (showingEvent && textBoxActive)
        {
            textBox.SetActive(true);
            if (eventLines.Count > 0)
            {
                if (eventLines[currentLine].team == 1)
                {
                    playerSpeakerPanel.SetActive(true);
                    enemySpeakerPanel.SetActive(false);
                    playerSpeaker.text = eventLines[currentLine].speaker;
                    playerImage.sprite =
                        ImageResourcesManager.getInstance().ReturnSprite(eventLines[currentLine].speaker);
                }
                else if (eventLines[currentLine].team == 2)
                {
                    playerSpeakerPanel.SetActive(false);
                    enemySpeakerPanel.SetActive(true);
                    enemySpeaker.text = eventLines[currentLine].speaker;
                    enemyImage.sprite = ImageResourcesManager.getInstance()
                        .ReturnSprite(eventLines[currentLine].speaker);
                }
                theText.text = eventLines[currentLine].line;
            }
            else
            {
                textBox.SetActive(false);
                showingEvent = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                eventLines.RemoveAt(0);
            }
        }
    }

    public void EventHandler(string speakerName, string eventName)
    {
        currentLine = 0;
        foreach (XmlNode node in eventDialogue.DocumentElement)
        {
            if (speakerName.Equals(node.Attributes[0].Value) && eventName.Equals(node.Attributes[2].Value))
            {
                eventLines.Add(new entry(speakerName, int.Parse(node.Attributes[1].Value), node.InnerText, eventName));
            }
                
        }
        showingEvent = true;
    }
}
