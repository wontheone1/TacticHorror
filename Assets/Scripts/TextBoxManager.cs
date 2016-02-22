using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour
{
    public GameObject TextBox, EnemySpeakerPanel, PlayerSpeakerPanel;
    public Text EnemySpeaker, PlayerSpeaker, TheText;
    public Image EnemyImage, PlayerImage;
    public TextAsset TextFile;
    public XmlDocument Dialogue = new XmlDocument();
    public XmlDocument EventDialogue = new XmlDocument();
    public string[] TextLines;
    private readonly List<Entry> _lines = new List<Entry>();
    private readonly List<Entry> _eventLines = new List<Entry>();
    private Statemachine _statemachine;
    public int CurrentLine, EndAtLine;
    public bool DialogueDone, ShowingEvent;
    public bool TextBoxActive = true;

    public class Entry
    {
        public string Speaker;
        public int Team;
        public string Line;
        public string EventName;

        public Entry(string speaker, int team, string line)
        {
            Speaker = speaker;
            Team = team;
            Line = line;
        }

        public Entry(string speaker, int team, string line, string eventName)
        {
            Speaker = speaker;
            Team = team;
            Line = line;
            EventName = eventName;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {

        try
        {
            TextBox = GameObject.Find("DialoguePanel");
            EnemySpeakerPanel = GameObject.Find("enemySpeakerPanel");
            PlayerSpeakerPanel = GameObject.Find("playerSpeakerPanel");
            EnemySpeaker = GameObject.Find("enemySpeaker").GetComponent<Text>();
            PlayerSpeaker = GameObject.Find("playerSpeaker").GetComponent<Text>();
            EnemyImage = GameObject.Find("enemyImage").GetComponent<Image>();
            PlayerImage = GameObject.Find("playerImage").GetComponent<Image>();
            TheText = GameObject.Find("Dialogue").GetComponent<Text>();
        }
        catch (Exception exception)
        {
            Debug.Log(exception);
            TextBoxActive = false;
        }
        _statemachine = GetComponent<Statemachine>();
    }

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        Dialogue.Load("Assets/text/Level1_Dialog.xml");
        EventDialogue.Load("Assets/text/Level1_Event_Dialog.xml");
        try
        {
            PlayerSpeakerPanel.SetActive(false);
            EnemySpeakerPanel.SetActive(false);
        }
        catch (Exception)
        {
            // ignored
        }

        if (Dialogue.DocumentElement != null)
        {
            Debug.Log("hello");
            foreach (XmlNode node in Dialogue.DocumentElement)
            {
                if (node.Attributes != null)
                {
                    Debug.Log("hello");
                    _lines.Add(new Entry(node.Attributes[0].Value, int.Parse(node.Attributes[1].Value), node.InnerText));
                }
            }
        }
            
        if (EndAtLine == 0)
        {
            EndAtLine = _lines.Count - 1;
        }
        if (TextBoxActive) return;
        EndAtLine = 0;
        _statemachine.StartGame();
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        if (!DialogueDone && TextBoxActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CurrentLine++;
            }

            if (CurrentLine > EndAtLine)
            {
                TextBox.SetActive(false);
                _statemachine.StartGame();
                DialogueDone = true;
            }
            else
            {
                if (_lines[CurrentLine].Team == 1)
                {
                    try
                    {
                        PlayerSpeakerPanel.SetActive(true);
                        EnemySpeakerPanel.SetActive(false);
                        PlayerSpeaker.text = _lines[CurrentLine].Speaker;
                        PlayerImage.sprite = ImageResourcesManager.GetInstance().ReturnSprite(_lines[CurrentLine].Speaker);
                    }
                    catch (Exception e) { Debug.Log(e); }

                }
                else if (_lines[CurrentLine].Team == 2)
                {
                    try
                    {
                        PlayerSpeakerPanel.SetActive(false);
                        EnemySpeakerPanel.SetActive(true);
                        EnemySpeaker.text = _lines[CurrentLine].Speaker;
                        EnemyImage.sprite = ImageResourcesManager.GetInstance().ReturnSprite(_lines[CurrentLine].Speaker);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                TheText.text = _lines[CurrentLine].Line;
            }
        }

        if (!ShowingEvent || !TextBoxActive) return;
        TextBox.SetActive(true);
        if (_eventLines.Count > 0)
        {
            if (_eventLines[CurrentLine].Team == 1)
            {
                PlayerSpeakerPanel.SetActive(true);
                EnemySpeakerPanel.SetActive(false);
                PlayerSpeaker.text = _eventLines[CurrentLine].Speaker;
                PlayerImage.sprite =
                    ImageResourcesManager.GetInstance().ReturnSprite(_eventLines[CurrentLine].Speaker);
            }
            else if (_eventLines[CurrentLine].Team == 2)
            {
                PlayerSpeakerPanel.SetActive(false);
                EnemySpeakerPanel.SetActive(true);
                EnemySpeaker.text = _eventLines[CurrentLine].Speaker;
                EnemyImage.sprite = ImageResourcesManager.GetInstance()
                    .ReturnSprite(_eventLines[CurrentLine].Speaker);
            }
            TheText.text = _eventLines[CurrentLine].Line;
        }
        else
        {
            TextBox.SetActive(false);
            ShowingEvent = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _eventLines.RemoveAt(0);
        }
    }

    public void EventHandler(string speakerName, string eventName)
    {
        CurrentLine = 0;
        if (EventDialogue.DocumentElement != null)
            foreach (XmlNode node in EventDialogue.DocumentElement.Cast<XmlNode>().Where(node => node.Attributes != null && (speakerName.Equals(node.Attributes[0].Value) && eventName.Equals(node.Attributes[2].Value))))
            {
                if (node.Attributes != null)
                    _eventLines.Add(new Entry(speakerName, int.Parse(node.Attributes[1].Value), node.InnerText, eventName));
            }
        ShowingEvent = true;
    }
}
