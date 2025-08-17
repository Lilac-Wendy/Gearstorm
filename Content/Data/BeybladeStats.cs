namespace Gearstorm.Content.Data;

// Bazinga //

public struct BeybladeStats(
    float mass = 0,
    float density = 0,
    float Height = 0,
    float Radius = 0,
    float tipFriction = 0,
    float spinSpeed = 0,
    float SpinDecay = 0,
    float balance = 0,
    float knockbackPower = 0,
    float knockbackResistance = 0,
    float moveSpeed = 0,
    float damageBase = 0)
{
    public float Mass = mass;
    public float Density = density;
    public float Height = Height;
    public float Radius = Radius;
    public float TipFriction = tipFriction;
    public float SpinSpeed = spinSpeed;
    public float SpinDecay = SpinDecay;
    public float Balance = balance;
    public float KnockbackPower = knockbackPower;
    public float KnockbackResistance = knockbackResistance;
    public float MoveSpeed = moveSpeed;
    public float DamageBase = damageBase;
}