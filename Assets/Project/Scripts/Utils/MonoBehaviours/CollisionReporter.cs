using UnityEngine;
using Scellecs.Morpeh;

public class CollisionReporter : MonoBehaviour
{
    [HideInInspector] public Entity Entity;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<CollisionReporter>(out var otherReporter))
        {
            if (this.GetInstanceID() > otherReporter.GetInstanceID())
            {
                return;
            }

            var world = this.Entity.GetWorld();
            var eventEntity = world.CreateEntity();
            ref var evt = ref world.GetStash<CollisionEventComponent>().Add(eventEntity);
            evt.Self = this.Entity;
            evt.Other = otherReporter.Entity;
        }
    }
}