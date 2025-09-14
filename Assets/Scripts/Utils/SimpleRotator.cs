using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField] private Vector3 _minAxisValues = Vector3.zero;
    [SerializeField] private Vector3 _maxAxisValues = Vector3.zero;
    [SerializeField] private Vector3 _axisSpeed = Vector3.one;

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    private bool _hasTarget = false;

    void Start()
    {
        _currentRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        // Если нет цели или достигли текущей цели, выбираем новую случайную цель
        if (!_hasTarget || HasReachedTarget())
        {
            SetNewRandomTarget();
        }

        // Плавно поворачиваем к цели
        RotateTowardsTarget();
    }

    private void SetNewRandomTarget()
    {
        _targetRotation = new Vector3(
            Random.Range(_minAxisValues.x, _maxAxisValues.x),
            Random.Range(_minAxisValues.y, _maxAxisValues.y),
            Random.Range(_minAxisValues.z, _maxAxisValues.z)
        );
        _hasTarget = true;
    }

    private bool HasReachedTarget()
    {
        float threshold = 1f; // Порог точности в градусах
        return Vector3.Distance(_currentRotation, _targetRotation) < threshold;
    }

    private void RotateTowardsTarget()
    {
        // Вычисляем направление к цели
        Vector3 direction = _targetRotation - _currentRotation;
        
        // Нормализуем углы для корректной работы с eulerAngles
        for (int i = 0; i < 3; i++)
        {
            if (direction[i] > 180f)
                direction[i] -= 360f;
            else if (direction[i] < -180f)
                direction[i] += 360f;
        }

        // Вычисляем шаг поворота для каждой оси
        Vector3 rotationStep = new Vector3(
            Mathf.Sign(direction.x) * Mathf.Min(Mathf.Abs(direction.x), _axisSpeed.x * Time.deltaTime),
            Mathf.Sign(direction.y) * Mathf.Min(Mathf.Abs(direction.y), _axisSpeed.y * Time.deltaTime),
            Mathf.Sign(direction.z) * Mathf.Min(Mathf.Abs(direction.z), _axisSpeed.z * Time.deltaTime)
        );

        // Применяем поворот
        _currentRotation += rotationStep;
        transform.rotation = Quaternion.Euler(_currentRotation);
    }
}
