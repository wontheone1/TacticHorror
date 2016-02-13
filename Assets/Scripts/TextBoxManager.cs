using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
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
    public string[] textLines;
    public List<entry> lines = new List<entry>();
    private Statemachine statemachine;
    public int currentLine;
    public int endAtLine;
    public bool dialogueDone = false;

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
        statemachine = GetComponent<Statemachine>();
    }

    void Start()
    {
        dialogue.Load("Assets/text/Level1_Dialog.xml");
        playerSpeakerPanel.SetActive(false);
        enemySpeakerPanel.SetActive(false);

        foreach (XmlNode node in dialogue.DocumentElement)
        {
            string Speaker = node.Attributes[0].Value;
            int team = int.Parse(node.Attributes[1].Value);
            lines.Add(new entry(node.Attributes[0].Value, int.Parse(node.Attributes[1].Value), node.InnerText));
        }
        if (endAtLine == 0)
        {
            endAtLine = lines.Count - 1;
        }
    }

    void Update()
    {
        if (!dialogueDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentLine++;
            }

            if (currentLine > endAtLine)
            {
                textBox.SetActive(false);
                statemachine.startGame();
                dialogueDone = true;
            }
            else
            {
                if (lines[currentLine].team == 1)
                {
                    playerSpeakerPanel.SetActive(true);
                    enemySpeakerPanel.SetActive(false);
                    playerSpeaker.text = lines[currentLine].speaker;
                    playerImage.sprite = ImageResourcesManager.getInstance().ReturnSprite(lines[currentLine].speaker);
                }
                else if (lines[currentLine].team == 2)
                {
                    playerSpeakerPanel.SetActive(false);
                    enemySpeakerPanel.SetActive(true);
                    enemySpeaker.text = lines[currentLine].speaker;
                    enemyImage.sprite = ImageResourcesManager.getInstance().ReturnSprite(lines[currentLine].speaker);
                }
                theText.text = lines[currentLine].line;
            }
        }
    }
}
