using TestGame;
using UnityEngine;
using Wargon.TinyEcs;

public class TriggerEmitter : MonoBehaviour {
    [SerializeField] private Entity _entity;
    private void Reset() {
        _entity = GetComponent<Entity>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out Entity e))
            _entity.Add(new OnTriggerEnterEvent{Other = e});
    }
}
