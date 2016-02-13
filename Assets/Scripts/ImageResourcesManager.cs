using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

public class ImageResourcesManager {

    private static ImageResourcesManager instance = new ImageResourcesManager();
    private Dictionary<string, Sprite> sprites;

    public static ImageResourcesManager getInstance()
    {
        return instance;
    }

    private ImageResourcesManager()
    {
        sprites = new Dictionary<string, Sprite>();
        Sprite soldierSprite = Resources.Load<Sprite>("soldier");
        sprites.Add("Vampire", Resources.Load<Sprite>("vampire"));
        sprites.Add("Frankenstein", Resources.Load<Sprite>("frankenstein"));
        sprites.Add("Werewolf", Resources.Load<Sprite>("werewolf"));
        sprites.Add("Ghost", Resources.Load<Sprite>("ghost"));
        sprites.Add("Soldier1", soldierSprite);
        sprites.Add("Soldier2", soldierSprite);
        sprites.Add("Soldier3", soldierSprite);
    }

    public Sprite ReturnSprite(string spriteName)
    {
        return sprites[spriteName];
    }
}
