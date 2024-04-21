namespace Sandbox.Ants.Aspects;

public struct Nutrients
{
    public int calories;
}

public struct Pheromone
{
    public int strength;
    public int initialStrength;
    public bool counted = false;

    public Pheromone()
    {
        strength = 0;
        initialStrength = 0;
    }
}

public struct FoundFoodThought
{
    public int thoughtAge = 0;

    public FoundFoodThought()
    {
    }
}

public struct AntAppearance
{
    public float red;
    public float green;
    public float blue;
}

public struct AntPhysique
{
    public float movespeed = 1;

    public AntPhysique()
    {
    }
}

public struct FoodAppearance
{ }

public struct WanderMovement
{
    public float direction;
    public int driftFrequency;
}

public struct DirectMovement
{
    public float targetX;
    public float targetY;
}

public struct BasicMovement
{
    public float direction;
}
