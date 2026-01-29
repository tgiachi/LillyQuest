using LillyQuest.RogueLike.Data.Scheduler;
using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.Tests.RogueLike.Scheduler;

[TestFixture]
public class TurnSchedulerTests
{
    #region Test Helpers

    private sealed class TestEntity : ISchedulerEntity
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int Energy { get; set; }
        public int Speed { get; set; } = 10;
        public bool IsPlayer { get; set; }
        public bool IsActive { get; set; } = true;

        private readonly Queue<ISchedulerAction> _actions = new();

        public void QueueAction(ISchedulerAction action) => _actions.Enqueue(action);

        public ISchedulerAction? GetNextAction()
        {
            return _actions.Count > 0 ? _actions.Dequeue() : null;
        }
    }

    private sealed class TestAction : ISchedulerAction
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int EnergyCost { get; set; } = 100;
        public ISchedulerEntity Actor { get; }
        public bool CanExecuteResult { get; set; } = true;
        public ActionResult ExecuteResult { get; set; } = ActionResult.Success;
        public int ExecuteCount { get; private set; }

        public TestAction(ISchedulerEntity actor)
        {
            Actor = actor;
        }

        public bool CanExecute() => CanExecuteResult;

        public ActionResult Execute()
        {
            ExecuteCount++;
            return ExecuteResult;
        }
    }

    #endregion

    [Test]
    public void Empty_Scheduler_Returns_Empty_State()
    {
        var scheduler = new TurnScheduler();

        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(result.State, Is.EqualTo(SchedulerState.Empty));
    }

    [Test]
    public void Player_With_No_Action_Returns_WaitingForPlayerInput()
    {
        var scheduler = new TurnScheduler();
        var player = new TestEntity { IsPlayer = true, Speed = 10, Energy = 100 };
        scheduler.AddEntity(player);

        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(result.State, Is.EqualTo(SchedulerState.WaitingForPlayerInput));
        Assert.That(result.ActiveEntity, Is.SameAs(player));
    }

    [Test]
    public void Player_Action_Executes_And_Deducts_Energy()
    {
        var scheduler = new TurnScheduler();
        var player = new TestEntity { IsPlayer = true, Speed = 10, Energy = 100 };
        var action = new TestAction(player) { EnergyCost = 100 };

        scheduler.AddEntity(player);

        // First call - player is ready (energy=100) but no action queued
        var result1 = scheduler.ProcessUntilPlayerInput();
        Assert.That(result1.State, Is.EqualTo(SchedulerState.WaitingForPlayerInput), "Should wait for player input");
        Assert.That(player.Energy, Is.EqualTo(100), "Player should have 100 energy");

        // Queue the action and process again
        scheduler.EnqueuePlayerAction(action);
        var result2 = scheduler.ProcessUntilPlayerInput();

        Assert.That(action.ExecuteCount, Is.EqualTo(1), $"Action should have been executed once. State={result2.State}");
        Assert.That(result2.ExecutedActions, Has.Count.EqualTo(1), "Should have one executed action");

        var executedRecord = result2.ExecutedActions[0];
        Assert.That(executedRecord.Result, Is.EqualTo(ActionResult.Success), "Action result should be Success");
        Assert.That(executedRecord.EnergyCostPaid, Is.EqualTo(100), "Energy cost should be 100");
        Assert.That(executedRecord.ActorEnergyAfter, Is.EqualTo(0), "Energy was 0 right after action");

        // After processing, player will have gained energy until ready again (100)
        // because ProcessUntilPlayerInput continues until player can act again
        Assert.That(result2.State, Is.EqualTo(SchedulerState.WaitingForPlayerInput), "Should wait for next input");
        Assert.That(player.Energy, Is.GreaterThanOrEqualTo(TurnScheduler.EnergyThreshold), 
            "Player should have enough energy to act again");
    }

    [Test]
    public void AI_Entity_Acts_Automatically()
    {
        var scheduler = new TurnScheduler();
        var ai = new TestEntity { IsPlayer = false, Speed = 10, Energy = 100 };
        var action = new TestAction(ai);
        ai.QueueAction(action);

        scheduler.AddEntity(ai);

        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(action.ExecuteCount, Is.EqualTo(1));
        Assert.That(ai.Energy, Is.EqualTo(0));
        Assert.That(result.State, Is.EqualTo(SchedulerState.Empty)); // No player, so empty
    }

    [Test]
    public void Faster_Entity_Acts_More_Often()
    {
        var scheduler = new TurnScheduler();

        // Player speed 10, enemy speed 20 (twice as fast)
        var player = new TestEntity { IsPlayer = true, Speed = 10 };
        var fastEnemy = new TestEntity { IsPlayer = false, Speed = 20 };

        var enemyActionCount = 0;
        var playerActionCount = 0;

        scheduler.AddEntity(player);
        scheduler.AddEntity(fastEnemy);

        // Simulate 20 player turns
        for (var i = 0; i < 20; i++)
        {
            // Queue many enemy actions so it can act multiple times
            for (var j = 0; j < 10; j++)
            {
                fastEnemy.QueueAction(new TestAction(fastEnemy));
            }

            var result = scheduler.ProcessUntilPlayerInput();

            if (result.State == SchedulerState.WaitingForPlayerInput)
            {
                // Count enemy actions that happened before player's turn
                enemyActionCount += result.ExecutedActions.Count(a => !a.Actor.IsPlayer);

                // Player acts
                var playerAction = new TestAction(player);
                scheduler.EnqueuePlayerAction(playerAction);
                var afterPlayerResult = scheduler.ProcessUntilPlayerInput();
                playerActionCount++;

                // Count any additional enemy actions after player
                enemyActionCount += afterPlayerResult.ExecutedActions.Count(a => !a.Actor.IsPlayer);
            }
        }

        // Fast enemy should have acted roughly twice as often
        // Allow some variance due to energy accumulation patterns
        Assert.That(enemyActionCount, Is.GreaterThan(playerActionCount));
        Assert.That((double)enemyActionCount / playerActionCount, Is.GreaterThan(1.5));
    }

    [Test]
    public void Energy_Accumulates_Until_Threshold()
    {
        var scheduler = new TurnScheduler();
        var entity = new TestEntity { IsPlayer = true, Speed = 10, Energy = 0 };

        scheduler.AddEntity(entity);

        // First call - entity has 0 energy, so energy gets distributed
        var result = scheduler.ProcessUntilPlayerInput();

        // After 10 ticks of speed 10, entity should have 100 energy
        Assert.That(result.State, Is.EqualTo(SchedulerState.WaitingForPlayerInput));
        Assert.That(entity.Energy, Is.EqualTo(100));
        Assert.That(scheduler.CurrentTick, Is.EqualTo(10));
    }

    [Test]
    public void Blocked_Action_Returns_PlayerActionBlocked()
    {
        var scheduler = new TurnScheduler();
        var player = new TestEntity { IsPlayer = true, Energy = 100 };
        var action = new TestAction(player) { CanExecuteResult = false };

        scheduler.AddEntity(player);
        scheduler.EnqueuePlayerAction(action);

        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(result.State, Is.EqualTo(SchedulerState.PlayerActionBlocked));
        Assert.That(player.Energy, Is.EqualTo(100)); // Energy not deducted
    }

    [Test]
    public void Cancelled_Action_Returns_PlayerActionCancelled()
    {
        var scheduler = new TurnScheduler();
        var player = new TestEntity { IsPlayer = true, Energy = 100 };
        var action = new TestAction(player) { ExecuteResult = ActionResult.Cancelled };

        scheduler.AddEntity(player);
        scheduler.EnqueuePlayerAction(action);

        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(result.State, Is.EqualTo(SchedulerState.PlayerActionCancelled));
        Assert.That(player.Energy, Is.EqualTo(100)); // Energy not deducted for cancelled
    }

    [Test]
    public void Inactive_Entity_Is_Removed()
    {
        var scheduler = new TurnScheduler();
        var entity = new TestEntity { IsPlayer = false, Energy = 100, IsActive = false };

        scheduler.AddEntity(entity);
        var result = scheduler.ProcessUntilPlayerInput();

        Assert.That(result.RemovedEntities, Has.Count.EqualTo(1));
        Assert.That(scheduler.EntityCount, Is.EqualTo(0));
    }

    [Test]
    public void Turn_Order_Is_Deterministic()
    {
        // Create multiple entities with same speed
        var scheduler = new TurnScheduler();
        var entities = Enumerable.Range(0, 5)
            .Select(_ => new TestEntity { Speed = 10, Energy = 100 })
            .ToList();

        foreach (var entity in entities)
        {
            scheduler.AddEntity(entity);
        }

        var firstOrder = scheduler.GetEntitiesByEnergy().Select(e => e.Id).ToList();

        // Reset and check order again
        scheduler.Reset();
        foreach (var entity in entities)
        {
            entity.Energy = 100;
        }

        var secondOrder = scheduler.GetEntitiesByEnergy().Select(e => e.Id).ToList();

        Assert.That(firstOrder, Is.EqualTo(secondOrder));
    }

    [Test]
    public void Higher_Energy_Acts_First_On_Same_Tick()
    {
        var scheduler = new TurnScheduler();

        var slow = new TestEntity { Speed = 8, Energy = 104 };  // Just over threshold
        var fast = new TestEntity { Speed = 12, Energy = 120 }; // More energy

        var actionOrder = new List<Guid>();

        slow.QueueAction(new TestAction(slow));
        fast.QueueAction(new TestAction(fast));

        scheduler.AddEntity(slow);
        scheduler.AddEntity(fast);
        scheduler.ActionExecuted += record => actionOrder.Add(record.Actor.Id);

        scheduler.ProcessUntilPlayerInput();

        Assert.That(actionOrder[0], Is.EqualTo(fast.Id)); // Fast (more energy) acts first
        Assert.That(actionOrder[1], Is.EqualTo(slow.Id));
    }

    [Test]
    public void Variable_Action_Costs_Affect_Turn_Order()
    {
        var scheduler = new TurnScheduler();

        // Player speed 10, enemy speed 10 (same speed)
        var player = new TestEntity { IsPlayer = true, Speed = 10, Energy = 100 };
        var enemy = new TestEntity { Speed = 10, Energy = 100 };

        var actionOrder = new List<Guid>();
        scheduler.ActionExecuted += record => actionOrder.Add(record.Actor.Id);

        scheduler.AddEntity(player);
        scheduler.AddEntity(enemy);

        // Player does a quick action (cost 50), enemy does normal (cost 100)
        var quickAction = new TestAction(player) { EnergyCost = 50 };
        var normalAction = new TestAction(enemy);
        enemy.QueueAction(normalAction);

        scheduler.EnqueuePlayerAction(quickAction);
        var result1 = scheduler.ProcessUntilPlayerInput();

        // Both should have acted
        Assert.That(actionOrder, Has.Count.EqualTo(2));
        Assert.That(actionOrder.Contains(player.Id), Is.True, "Player should have acted");
        Assert.That(actionOrder.Contains(enemy.Id), Is.True, "Enemy should have acted");

        // Now queue another player action - player should be ready sooner than enemy
        actionOrder.Clear();
        
        var playerAction2 = new TestAction(player) { EnergyCost = 100 };
        enemy.QueueAction(new TestAction(enemy));
        
        scheduler.EnqueuePlayerAction(playerAction2);
        var result2 = scheduler.ProcessUntilPlayerInput();

        Assert.That(actionOrder[0], Is.EqualTo(player.Id), "Player should act first due to lower energy cost");
    }

    [Test]
    public void AI_Without_Action_Skips_Turn()
    {
        var scheduler = new TurnScheduler();
        var ai = new TestEntity { Speed = 10, Energy = 100 };
        // Don't queue any action

        scheduler.AddEntity(ai);
        scheduler.ProcessUntilPlayerInput();

        // AI should have lost 100 energy (standard skip cost)
        Assert.That(ai.Energy, Is.EqualTo(0));
    }

    [Test]
    public void Events_Fire_Correctly()
    {
        var scheduler = new TurnScheduler();
        var player = new TestEntity { IsPlayer = true, Energy = 100 };
        var action = new TestAction(player);

        var actingFired = false;
        var executedFired = false;

        scheduler.EntityActing += _ => actingFired = true;
        scheduler.ActionExecuted += _ => executedFired = true;

        scheduler.AddEntity(player);
        scheduler.EnqueuePlayerAction(action);
        scheduler.ProcessUntilPlayerInput();

        Assert.That(actingFired, Is.True);
        Assert.That(executedFired, Is.True);
    }
}
