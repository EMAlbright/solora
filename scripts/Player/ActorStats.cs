using Godot;

public partial class ActorStats: Node
{
    [Signal]
    public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

    [Signal]
    public delegate void StaminaChangedEventHandler(float currentStamina, float maxStamina);

    [Signal]
    public delegate void DamagedEventHandler(float amount);

    [Signal]
    public delegate void DeathEventHandler();


    [ExportGroup("Stamina")]
    [Export]
	public float MaxStamina {get; private set; } = 100000;
    
    [Export]
    public float StaminaRegenAmt{get; private set; } = 5f;

    [Export]
    public float StaminaRegenInterval {get; private set; } = 1f;

    [ExportGroup("Health")]
    [Export]
    public float MaxHealth {get; private set; }= 100f;
    
    [Export]
    public float HealthRegenAmt{get; private set; } = 5f;

    [Export]
    public float HealthRegenInterval {get; private set; } = 1f;

    [ExportGroup("Damage")]
    [Export]
    public float InvulnerabilityInterval {get; private set; } = 1f;

    public float CurrentHealth {get; private set; }
    public float CurrentStamina{get; private set; }
    public bool IsInvulnerable{get; private set;}

    public bool IsAlive => CurrentHealth > 0f;

    private Timer _healthRegenTimer;
    private Timer _staminaRegenTimer;
    private Timer _invulnerabilityTimer;



    public override void _Ready()
    {
        CreateTimers();
    }

    // Player Timers
    public void CreateTimers()

    {
        _healthRegenTimer = new Timer
        {
            WaitTime = HealthRegenInterval,
            OneShot = true
        };

        _staminaRegenTimer = new Timer
        {
            WaitTime = StaminaRegenInterval,
            OneShot = true
        };

        _invulnerabilityTimer = new Timer
        {
            WaitTime = InvulnerabilityInterval,
            OneShot = true
        };

        AddChild(_healthRegenTimer);
        AddChild(_staminaRegenTimer);
        AddChild(_invulnerabilityTimer);
		
    }

    // Actor Take Damage
    public void TakeDamage(
        float amount,
        bool ignoreInvulnerability = false
    )
    {
        if (!IsAlive) return;

        if (amount <= 0f) return;
        
        if (IsInvulnerable && !ignoreInvulnerability) return;
        
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);

        EmitSignal(SignalName.Damaged, amount);

        EmitHealthChanged();

        if (CurrentHealth <= 0f)
        {
            Die();
            return;
        }

        if (!ignoreInvulnerability)
        {
            StartInvulnerability();
        }
    }

    // Actor Invulnerability
    private void StartInvulnerability()
    {
        IsInvulnerable = true;
        _invulnerabilityTimer.Start();
    }

    private void OnInvulnerabilityFinished()
    {
        IsInvulnerable = false;
    }

    // Actor Health
    public void Heal(float amount)
    {
        if(!IsAlive || amount < 0f)
        {
            return;
        } 

        CurrentHealth = Mathf.Min(CurrentHealth+amount, MaxHealth);
        EmitHealthChanged();
    }

    
    // Actor Stamina
    public void RestoreStamina(float amount)
    {
        if(!IsAlive || amount < 0f)
        {
            return;
        } 

        CurrentHealth = Mathf.Min(CurrentStamina+amount, MaxStamina);
        EmitStaminaChanged();
    }
    public bool TryUseStamina(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (CurrentStamina < amount)
        {
            return false;
        }

        CurrentStamina -= amount;
        EmitStaminaChanged();

        return true;  
    }

    public void UseStamina(float amount)
    {
        if(amount <= 0f) return;

        CurrentStamina = Mathf.Max(CurrentStamina - amount, MaxStamina);
        EmitStaminaChanged();
    }

    // Actor Death
    private void Die()
    {
        EmitSignal(SignalName.Death);
    }

    // Actor Tmers
    private void OnHealthRegenTimer()
    {
        if(!IsAlive) return;

        if(CurrentHealth >= MaxHealth) return;

        Heal(HealthRegenAmt);
    }

    private void OnStaminaRegenTimer()
    {
        if(!IsAlive) return;

        if(CurrentStamina >= MaxStamina) return;

        RestoreStamina(StaminaRegenAmt);
    }

    // Actor Emitters
    private void EmitHealthChanged()
    {
        EmitSignal(
            SignalName.HealthChanged,
            CurrentHealth,
            MaxHealth
        );
    }

    private void EmitStaminaChanged()
    {
        EmitSignal(
            SignalName.StaminaChanged,
            CurrentStamina,
            MaxStamina
        );
    }
}