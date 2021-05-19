using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public PlayerManager player;

    public void UpdatePlayerCharacter(string i)
    {
        player.SetVisuals(int.Parse(i));
    }
}
