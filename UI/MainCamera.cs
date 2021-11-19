using UnityEngine;
using Industry.World.Map;

namespace Industry.UI
{
    [RequireComponent(typeof(Camera))]
    public class MainCamera : Singleton<MainCamera>
    {
        [SerializeField]
        private bool m_freeCamera;
        [SerializeField]
        private bool m_canMove;
        [SerializeField]
        private bool m_canZoom;
        [SerializeField]
        private float m_minHeight;
        [SerializeField]
        private float m_maxHeight;
        [SerializeField]
        private float m_moveSpeed;
        [SerializeField]
        private float m_rotationSpeed;
        [SerializeField]
        private float m_zoomSpeed;
        [SerializeField]
        private float m_zoomGravity;
        
        private float m_lastZoom;
        private float m_zoomDecrement;

        private float m_targetRotation;
        private int m_rotation;

        private Vector3 m_defaultRotation;
        private Vector3 m_rotationPoint;

        private bool AtTop
        {
            get
            {
                float pos = Input.mousePosition.y;
                float hgt = Screen.height;

                return pos == Mathf.Clamp(pos, hgt - 4f, hgt);
            }
        }
        private bool AtBottom
        {
            get
            {
                float pos = Input.mousePosition.y;

                return pos == Mathf.Clamp(pos, 0f, 4f);
            }
        }
        private bool AtRight
        {
            get
            {
                float pos = Input.mousePosition.x;
                float wdz = Screen.width;

                return pos == Mathf.Clamp(pos, wdz - 4f, wdz);
            }
        }
        private bool AtLeft
        {
            get
            {
                float pos = Input.mousePosition.x;

                return pos == Mathf.Clamp(pos, 0f, 4f);
            }
        }

        public bool CanMove
        {
            get
            {
                return m_canMove;
            }
            set
            {
                m_canMove = value;
            }
        }
        public bool CanZoom
        {
            get
            {
                return m_canZoom;
            }
            set
            {
                m_canZoom = value;
            }
        }
        public float MinHeight
        {
            get
            {
                return m_minHeight;
            }
            set
            {
                m_minHeight = value;
            }
        }
        public float MaxHeight
        {
            get
            {
                return m_maxHeight;
            }
            set
            {
                m_maxHeight = value;
            }
        }
        public float MoveSpeed
        {
            get
            {
                return m_moveSpeed;
            }
            set
            {
                m_moveSpeed = value;
            }
        }
        public float ZoomSpeed
        {
            get
            {
                return m_zoomSpeed;
            }
            set
            {
                m_zoomSpeed = value;
            }
        }
        public float ZoomGravity
        {
            get
            {
                return m_zoomGravity;
            }
            set
            {
                m_zoomGravity = value;
            }
        }
        
        private void CheckRotation()
        {
            if (m_freeCamera)
            {
                if (Input.GetMouseButton(1))
                {
                    float rotation = Input.GetAxis("Mouse X");
                    float inclination = Input.GetAxis("Mouse Y");

                    transform.Rotate(new Vector3(-inclination, rotation, 0));

                    float x = transform.rotation.eulerAngles.x;
                    if (x > 269f) x -= 360f;
                    if (x < 10f) x = 10f;
                    else if (x > 80f) x = 80f;

                    transform.rotation = Quaternion.Euler(new Vector3(x, transform.rotation.eulerAngles.y, 0));
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (!GetRotationPoint(out m_rotationPoint))
                        return;

                    m_rotation = -1;

                    m_targetRotation = (m_targetRotation + 270f) % 360f;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!GetRotationPoint(out m_rotationPoint))
                        return;
                    m_rotation = 1;

                    m_targetRotation = (m_targetRotation + 90f) % 360f;
                }

                if (m_rotation == 0)
                    return;

                float m = m_rotationSpeed * Time.deltaTime;
                transform.RotateAround(m_rotationPoint, Vector3.up, m * m_rotation);

                if (Mathf.Abs(transform.rotation.eulerAngles.y - m_targetRotation) < m * 0.5f)
                {
                    m_rotation = 0;
                    transform.rotation = Quaternion.Euler(m_defaultRotation.x, m_targetRotation, m_defaultRotation.z);
                }
            }
        }

        private bool GetRotationPoint(out Vector3 point)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

            if (Physics.Raycast(ray, out RaycastHit hit, WorldMap.Bounds.width * WorldMap.Bounds.height, 1 << 8))
            {
                point = hit.point;
                point.y = 0f;
                return true;
            }
            else
            {
                point = Vector3.zero;
                return false;
            }
        }

        private void CheckMovement()
        {
            Vector3 direction = new Vector3();

            bool at_top = AtTop || Input.GetKey(KeyCode.W), 
                at_bottom = AtBottom || Input.GetKey(KeyCode.S), 
                at_right = AtRight || Input.GetKey(KeyCode.D), 
                at_left = AtLeft || Input.GetKey(KeyCode.A);

            float limitX = WorldMap.Bounds.max.x;
            float limitZ = WorldMap.Bounds.max.z;

            direction.x = at_left ? -1 : at_right ? 1 : 0;
            direction.z = at_top ? 1 : at_bottom ? -1 : 0;
            
            direction = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * (direction * m_moveSpeed / MaxHeight * transform.position.y * 3f * Time.deltaTime);
            direction = transform.InverseTransformDirection(direction);

            transform.Translate(direction, Space.Self);

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -limitX, limitX), 
                transform.position.y, 
                Mathf.Clamp(transform.position.z, -limitZ, limitZ));
        }
        
        private void CheckZoom()
        {
            bool zoom_num_in  = Input.GetKeyDown(KeyCode.KeypadPlus)  || Input.GetKey(KeyCode.KeypadPlus)  || Input.GetKeyDown(KeyCode.Plus)  || Input.GetKey(KeyCode.Plus);
            bool zoom_num_out = Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus) || Input.GetKey(KeyCode.Minus);
            
            float zoom = zoom_num_in ? 1 : zoom_num_out ? -1 : Input.GetAxis("Mouse ScrollWheel");
            
            if (zoom > 0)
                m_lastZoom = 10.0f;
            else if (zoom < 0)
                m_lastZoom = -10.0f;
            m_zoomDecrement = Mathf.Sign(m_lastZoom) * m_zoomGravity;

            Vector3 zPos = Vector3.forward * m_zoomSpeed * m_lastZoom * Time.deltaTime;

            if (m_lastZoom != 0f)
                transform.Translate(zPos);

            if (transform.position.y < m_minHeight || transform.position.y > m_maxHeight)
            {
                transform.Translate(-zPos);
                m_lastZoom = 0f;
            }

            if ((int)(m_lastZoom * 100) != 0)
                m_lastZoom -= m_zoomDecrement;
            else
                m_lastZoom = 0f;
        }

        private void Awake()
        {
            m_defaultRotation = transform.rotation.eulerAngles;
            m_targetRotation = m_defaultRotation.y;
        }

        private void Update()
        {
            CheckRotation();

            if (m_canMove)
                CheckMovement();

            if (m_canZoom)
                CheckZoom();
        }
    }
}
