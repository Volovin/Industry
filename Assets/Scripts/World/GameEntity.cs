using System.Collections;
using UnityEngine;

namespace Industry.World
{
    public abstract class GameEntity : MonoBehaviour
    {
        public bool IsAlive
        {
            get; protected set;
        }

        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }


        private void Awake()
        {
            IsAlive = true;

            _Awake();
        }

        private void Start()
        {
            _Start();
        }

        private void Update()
        {
            _Update();
        }

        private void OnMouseDown()
        {
            if (!(this is IClickable))
                return;

            (this as IClickable).OnClick();
        }


        public virtual void Destroy()
        {
            if (!IsAlive || gameObject == null)
                return;

            IsAlive = false;
            enabled = false;

            Name = "[DESTROYED] " + gameObject.name;
            
            Object.Destroy(gameObject);
        }


        protected abstract void _Awake();
        protected abstract void _Start();
        protected abstract void _Update();
    }
}
