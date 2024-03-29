using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerMovement : MonoBehaviour
{

    public UnityAction<MovementState> OnMovementStateChanged;

    [SerializeField] private PointsNavigator _navigator;
    [SerializeField] private MovementState _movementState;

    [SerializeField] private Point _currentPoint;
    [SerializeField] private Button _buttonStart;
    [SerializeField, Min(0)] private float _speed = 3.5f;

    private NavMeshAgent _agent;
    private PlayerCombat _combat;
    private Level _level;
    
    public MovementState MovementState => _movementState;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;
        _combat = GetComponent<PlayerCombat>();
        _combat.enabled = false;
    }

    public void Constructor(PointsNavigator navigator, Level level) 
    {
        _navigator = navigator;
        _level = level;
        _level.OnLaunch += StartMove;
    }


    private void StartMove() 
    {
        _level.OnLaunch -= StartMove;
        _combat.enabled = true;

        _currentPoint = _navigator.FirstPoint;
        UpdateAgentDestination(_currentPoint.transform.position);
        _currentPoint.OnComplete += MoveNextPoint;
    }

    private void MoveNextPoint() 
    {
        _currentPoint.OnComplete -= MoveNextPoint;

        if (_navigator.LastPoint == _currentPoint) 
            return;

        _currentPoint = _navigator.GetNextPoint(_currentPoint);
        UpdateAgentDestination(_currentPoint.transform.position);
        _currentPoint.OnComplete += MoveNextPoint;
    }

    private void UpdateAgentDestination(Vector3 target)
    {
        _agent.SetDestination(target);
        UpdateMovementState(MovementState.Move);
    }

    private void UpdateMovementState(MovementState state) 
    {
        _movementState = state;
        OnMovementStateChanged?.Invoke(state);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Point point)) 
        {
            if (!point.Complete)
                UpdateMovementState(MovementState.Idle);
            else
                MoveNextPoint();
        }
    }

}
