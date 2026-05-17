using UnityEngine;

public class EnemyContoroller : CellObject
{
    public int MaxHealth = 3;
    public int DamagePerHit = 1;
    public int FoodDamageOnAttack = 1;
    private int m_Health;
    private Animator m_Animator;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        m_Health = Mathf.Max(1, MaxHealth);
        m_Animator = GetComponent<Animator>();
    }

    public override bool PlayerWantsToEnter()
    {
        return false;
    }

    public override bool PlayerBumped()
    {
        m_Health -= DamagePerHit;

        if (m_Health <= 0)
        {
            DestroySelf();
            return true;
        }

        return false;
    }

    public void AttackPlayer()
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Attack");
        }

        GameManager.Instance.ChangeFood(-FoodDamageOnAttack);
    }
}
