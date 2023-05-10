using UnityEngine;
using Wargon.TestGame;
using Wargon.TinyEcs;

public class CollisionEmitter : MonoBehaviour {
    [SerializeField] private Entity _entity;

    private void Reset() {
        _entity = GetComponent<Entity>();
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent(out Entity e))
            _entity.Add(new OnTriggerEnterEvent { Other = e });
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Entity e))
            _entity.Add(new OnTriggerEnterEvent { Other = e });
    }
}