namespace Gearstorm.Content.Data;

public struct BeybladeStats
{
    public float Mass;
    public float Density;
    public float Height;
    public float Radius;
    public float TipFriction;
    public float SpinSpeed;
    public float SpinDecay;
    public float Balance;
    public float KnockbackPower;
    public float KnockbackResistance;
    public float MoveSpeed;
    public float DamageBase;

    public BeybladeStats(
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
        Mass = mass;
        Density = density;
        this.Height = Height;
        this.Radius = Radius;
        TipFriction = tipFriction;
        SpinSpeed = spinSpeed;
        this.SpinDecay = SpinDecay;
        Balance = balance;
        KnockbackPower = knockbackPower;
        KnockbackResistance = knockbackResistance;
        MoveSpeed = moveSpeed;
        DamageBase = damageBase;
    }

    public float MomentOfInertia =>
        0.5f * Mass * Radius * Radius * (1f + Density * 0.3f);
}