public class StunEffect : Effect
{
    private NetworkMovementComponent movement;
    private InputComponent input;
    private AttackComponent attack;

    public StunEffect(Entity target, float duration)
        : base(target, duration)
    {
        movement = target.GetComponent<NetworkMovementComponent>();
        input = target.GetComponent<InputComponent>();
        attack = target.GetComponent<AttackComponent>();
    }

    public override void Apply()
    {
        if (movement != null) movement.enabled = false;
        if (input != null) input.enabled = false;
        if (attack != null) attack.enabled = false;
    }

    public override void Remove()
    {
        if (movement != null) movement.enabled = true;
        if (input != null) input.enabled = true;
        if (attack != null) attack.enabled = true;
    }
}