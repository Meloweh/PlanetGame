using UnityEngine;

public class Planet
{
    public Vector2 Position { get; private set; }
    public float Radius { get; private set; }
    public float DragRadius { get; private set; }
    public int Mass { get; private set; }  // Proportional to the number of absorbed particles

    private const float BASE_RADIUS = 1.0f;  // Base visual radius
    private const float DRAG_RADIUS_MULTIPLIER = 5.0f;  // Multiplier to determine drag radius based on visual radius

    public Planet(Vector2 initialPosition)
    {
        Position = initialPosition;
        Radius = BASE_RADIUS;
        DragRadius = Radius * DRAG_RADIUS_MULTIPLIER;
        Mass = 0;
    }

    // Called when a particle is absorbed by the planet
    public void AbsorbParticle()
    {
        Mass += 1;
        UpdateRadiusAndDragRadius();
    }

    private void UpdateRadiusAndDragRadius()
    {
        // Increase radius based on mass (this is a simple linear relation; you can use more complex formulas)
        Radius = BASE_RADIUS + Mass * 0.1f;
        DragRadius = Radius * DRAG_RADIUS_MULTIPLIER;
    }
}