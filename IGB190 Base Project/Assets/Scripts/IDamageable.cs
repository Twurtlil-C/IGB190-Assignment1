public interface IDamageable
{
    void TakeDamage(float amount);
    void Kill();
    float GetCurrentHealthPercent();

}