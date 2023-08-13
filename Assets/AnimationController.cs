using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterAttachment
    {
        public SortingGroup attachment;
        public Vector3 downPosition;
        public Vector3 downRightPosition;
        public Vector3 rightPosition;
        public Vector3 upRightPosition;
        public Vector3 upPosition;
        public Vector3 upLeftPosition;
        public Vector3 leftPosition;
        public Vector3 downLeftPosition;
    }

    [SerializeField] private CustomSpriteResolver _character;
    [SerializeField] private CustomSpriteResolver _helmet;
    [SerializeField] private CustomSpriteResolver _bodyArmor;
    [SerializeField] private CharacterAttachment[] _characterAttachments;
    [SerializeField] private SortingGroup _gun;
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
        Health h = GetComponent<Health>();
        if (h != null)
            h.RegisterOnDeath(Die);

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

                foreach (var ca in _characterAttachments)
                {
                    ca.attachment.transform.localPosition = ca.upPosition;
                    ca.attachment.transform.localScale = new Vector3(-1, 1, 1);
                    ca.attachment.sortingOrder = Mathf.RoundToInt(ca.upPosition.z);
                }
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

                foreach (var ca in _characterAttachments)
                {
                    ca.attachment.transform.localPosition = direction.x < 0 ? ca.upLeftPosition : ca.upRightPosition;
                    ca.attachment.transform.localScale = new Vector3(-1, 1, 1);
                    ca.attachment.sortingOrder = Mathf.RoundToInt((direction.x < 0 ? ca.upLeftPosition : ca.upRightPosition).z);
                }
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

                foreach (var ca in _characterAttachments)
                {
                    ca.attachment.transform.localPosition = direction.x < 0 ? ca.leftPosition : ca.rightPosition;
                    ca.attachment.transform.localScale = Vector3.one;
                    ca.attachment.sortingOrder = Mathf.RoundToInt((direction.x < 0 ? ca.leftPosition : ca.rightPosition).z);
                }
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

                foreach (var ca in _characterAttachments)
                {
                    ca.attachment.transform.localPosition = direction.x < 0 ? ca.downLeftPosition : ca.downRightPosition;
                    ca.attachment.transform.localScale = Vector3.one;
                    ca.attachment.sortingOrder = Mathf.RoundToInt((direction.x < 0 ? ca.downLeftPosition : ca.downRightPosition).z);
                }
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

            foreach (var ca in _characterAttachments)
            {
                ca.attachment.transform.localPosition = ca.downPosition;
                ca.attachment.transform.localScale = Vector3.one;
                ca.attachment.sortingOrder = Mathf.RoundToInt(ca.downPosition.z);
            }
        }

        float speedMultiplier = Vector2.Dot(_movement.GetMove().normalized, _aimSector) < 0 ? -_walkSpeed : _walkSpeed;
        _animator.SetFloat("Speed", _movement.GetMove().magnitude * speedMultiplier);

        _gun.transform.localScale = direction.x < 0 ? new Vector3(1, -1, 1) : Vector3.one;
    }

    void Die()
    {
        _animator.SetFloat("Speed", 0);
        _animator.Update(Time.deltaTime);
        _animator.enabled = false;
        this.enabled = false;
    }
}
