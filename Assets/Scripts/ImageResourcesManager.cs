using UnityEngine;
using System.Collections.Generic;

public class ImageResourcesManager {

    private static readonly ImageResourcesManager Instance = new ImageResourcesManager();
    private readonly Dictionary<string, Sprite> _sprites;

    public static ImageResourcesManager GetInstance()
    {
        return Instance;
    }

    private ImageResourcesManager()
    {
        _sprites = new Dictionary<string, Sprite>();
        Sprite soldierSprite = Resources.Load<Sprite>("soldier");
        _sprites.Add("Vampire", Resources.Load<Sprite>("vampire"));
        _sprites.Add("Frankenstein", Resources.Load<Sprite>("frankenstein"));
        _sprites.Add("Werewolf", Resources.Load<Sprite>("werewolf"));
        _sprites.Add("Ghost", Resources.Load<Sprite>("ghost"));
        _sprites.Add("Soldier1", soldierSprite);
        _sprites.Add("Soldier2", soldierSprite);
        _sprites.Add("Soldier3", soldierSprite);
    }

    public Sprite ReturnSprite(string spriteName)
    {
        return _sprites[spriteName];
    }
}
