using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private CustomSpriteResolver _character;
    [SerializeField] private CustomSpriteResolver _helmet;
    [SerializeField] private CustomSpriteResolver _bodyArmor;
    [SerializeField] private SpriteRenderer _gun;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _walkSpeed;

    private GunController _gunController;
    private TopDownMovement _movement;

    private Vector2 _aimSector;

    // Start is called before the first frame update
    void Start()
    {
        _gunController = GetComponent<GunController>();
        _movement = GetComponent<TopDownMovement>();

        if (_character != null)
            _character.Category = "Right";
        if (_helmet != null)
        {
            _helmet.Category = "Right";
            _helmet.Label = "Helmet";
        }
        if (_bodyArmor != null)
        {
            _bodyArmor.Category = "Right";
            _bodyArmor.Label = "BodyArmor";
        }
        _gun.sortingOrder = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = _gunController.GetAimVector();
        if (direction.y > Mathf.Abs(direction.x) * 2)
        {
            if (_character.Category != "Up")
            {
                _character.Category = "Up";
                _character.SetFlip(false);

                _helmet.Category = "Up";
                _helmet.SetFlip(false);

                _bodyArmor.Category = "Up";
                _bodyArmor.SetFlip(false);

                _aimSector = Vector2.up;

                if (_gun.sortingOrder != -1)
                    _gun.sortingOrder = -1;
            }
        }
        else if (direction.y > Mathf.Abs(direction.x) * 0.5f)
        {
            if (_character.Category != "UpRight" || (_aimSector.x < 0 && direction.x > 0) || (_aimSector.x > 0 && direction.x < 0))
            {
                _character.Category = "UpRight";
                _character.SetFlip(direction.x < 0);

                _helmet.Category = "UpRight";
                _helmet.SetFlip(direction.x < 0);

                _bodyArmor.Category = "UpRight";
                _bodyArmor.SetFlip(direction.x < 0);

                _aimSector = (Vector2.right * (direction.x < 0 ? -1 : 1) + Vector2.up).normalized;

                if (_gun.sortingOrder != -1)
                    _gun.sortingOrder = -1;
            }
        }
        else if (direction.y > -Mathf.Abs(direction.x) * 0.5f)
        {
            if (_character.Category != "Right" || (_aimSector.x < 0 && direction.x > 0) || (_aimSector.x > 0 && direction.x < 0))
            {
                _character.Category = "Right";
                _character.SetFlip(direction.x < 0);

                _helmet.Category = "Right";
                _helmet.SetFlip(direction.x < 0);

                _bodyArmor.Category = "Right";
                _bodyArmor.SetFlip(direction.x < 0);

                _aimSector = Vector2.right * (direction.x < 0 ? -1 : 1);

                if (_gun.sortingOrder != 1)
                    _gun.sortingOrder = 1;
            }
        }
        else if (direction.y > -Mathf.Abs(direction.x) * 2)
        {
            if (_character.Category != "DownRight" || (_aimSector.x < 0 && direction.x > 0) || (_aimSector.x > 0 && direction.x < 0))
            {
                _character.Category = "DownRight";
                _character.SetFlip(direction.x < 0);

                _helmet.Category = "DownRight";
                _helmet.SetFlip(direction.x < 0);

                _bodyArmor.Category = "DownRight";
                _bodyArmor.SetFlip(direction.x < 0);

                _aimSector = (Vector2.right * (direction.x < 0 ? -1 : 1) + Vector2.down).normalized;

                if (_gun.sortingOrder != 1)
                    _gun.sortingOrder = 1;
            }
        }
        else if (_character.Category != "Down")
        {
            _character.Category = "Down";
            _character.SetFlip(false);

            _helmet.Category = "Down";
            _helmet.SetFlip(false);

            _bodyArmor.Category = "Down";
            _bodyArmor.SetFlip(false);

            _aimSector = Vector2.down;

            if (_gun.sortingOrder != 1)
                _gun.sortingOrder = 1;
        }

        float speedMultiplier = Vector2.Dot(_movement.GetMove().normalized, _aimSector) < 0 ? -_walkSpeed : _walkSpeed;
        _animator.SetFloat("Speed", _movement.GetMove().magnitude * speedMultiplier);

        _gun.flipY = direction.x < 0;
    }
}
