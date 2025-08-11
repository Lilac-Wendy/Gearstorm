namespace Gearstorm.Content.Data
{
    public struct BeybladeStats(
        float mass = 0,
        float density = 0,
        float height = 0,
        float radius = 0,
        float tipFriction = 0,
        float spinSpeed = 0,
        float spinDecay = 0,
        float balance = 0,
        float knockbackPower = 0,
        float knockbackResistance = 0,
        float moveSpeed = 0,
        float damageBase = 0)
    {
        public float Mass = mass;
        public float Density = density;
        public float Height = height;
        public float Radius = radius;
        public float TipFriction = tipFriction;
        public float SpinSpeed = spinSpeed;
        public float SpinDecay = spinDecay;
        public float Balance = balance;
        public float KnockbackPower = knockbackPower;
        public float KnockbackResistance = knockbackResistance;
        public float MoveSpeed = moveSpeed;
        public float DamageBase = damageBase;
    }
}