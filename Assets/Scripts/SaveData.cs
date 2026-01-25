using System;
using UnityEngine;

public class SaveData
{
    public bool GameIsOver;
    
    ///Field
    public bool[] CellIsFree;

    ///FieldGraphics
    public SerializableColor[] SpriteRenderersColors;
    
    /// ScoreManager
    public int Score;   
    /// ScoreManager
    public int Combo;
    /// ScoreManager
    public int ComboReset;
    /// ScoreManager
    public int BestScore;
    
    /// UIManager
    public int LastCombo;
    /// UIManager
    public bool IsCombo;
}

[Serializable]
public struct SerializableColor
{
    public float r, g, b, a;

    public SerializableColor(Color color)
    {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
    }

    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}