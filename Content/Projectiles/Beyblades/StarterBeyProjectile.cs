namespace Gearstorm.Content.Projectiles.Beyblades;
// Bazinga //
public class StarterBeyProjectile : BaseBeybladeProjectile
{
    public override string Texture => "Gearstorm/Assets/Projectiles/StarterBeyProjectile";
    public static readonly BeybladeStats DefaultStats = new BeybladeStats(
        mass: 1.0f,          
        density: 1.00f,       
        radius: 0.5f,         
        height: 0.5f,         
        tipFriction: 0.020f,  
        spinSpeed: 1.5f,      
        spinDecay: 0.010f,     
        balance: 0.85f,       
        knockbackPower: 3.0f,  
        knockbackResistance: 1.8f, 
        moveSpeed: 1.0f,       
        damageBase: 15f       
    );

    public override void SetDefaults()
    {
        stats = DefaultStats;
        base.SetDefaults();
            
        // Configurações visuais específicas
        Projectile.scale = 1.0f;
    }
}