using UnityEngine;

public class PlayerEntity : CharacterEntity
{
    protected override void UpdateLogic()
    {
        if (IsOwner)
        {
            // Обробка введення від гравця
            HandleInput();
        }
    }

    private void HandleInput()
    {
        // Реалізація управління гравцем
    }
}