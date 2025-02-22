using UnityEngine;

namespace MBaske.Sensors.Grid
{
    /// <summary>
    /// Detects gameobjects located on a plane.
    /// </summary>
    public class GameObjectDetector2D : GameObjectDetector
    {
        /// <summary>
        /// Whether and how to rotate detection bounds with the sensor component.
        /// </summary>
        public enum SensorRotationType
        {
            AgentY, AgentXYZ, None
        }

        /// <summary>
        /// Whether and how to rotate detection bounds with the sensor component.
        /// </summary>
        public SensorRotationType RotationType
        {
            set { m_RotationType = value; }
        }
        private SensorRotationType m_RotationType;

        /// <summary>
        /// The world rotation to use if <see cref="Detector2DRotationType"/>
        /// is set to <see cref="Detector2DRotationType.None"/>.
        /// </summary>
        public Quaternion WorldRotation
        {
            set { m_WorldRotation = value; }
        }
        private Quaternion m_WorldRotation;

        /// <summary>
        /// Constraint for 2D detection.
        /// </summary>
        public DetectionConstraint2D Constraint
        {
            set
            {
                m_Constraint = value;
                m_BoundsCenter = value.Bounds.center;
                m_BoundsExtents = value.Bounds.extents;
            }
        }
        private DetectionConstraint2D m_Constraint;
        private Vector3 m_BoundsCenter;
        private Vector3 m_BoundsExtents;

        /// <inheritdoc/>
        public override void OnSensorUpdate()
        {
            base.OnSensorUpdate();

            Quaternion rotation = m_WorldRotation;

            switch (m_RotationType)
            {
                case SensorRotationType.AgentY:
                    rotation = Quaternion.AngleAxis(m_Transform.eulerAngles.y, Vector3.up);
                    break;
                case SensorRotationType.AgentXYZ:
                    rotation = m_Transform.rotation;
                    break;
            }

            Vector3 position = m_Transform.position;
            Matrix4x4 worldToLocalMatrix = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;

            int numFound = 0;
            do
            {
                ValidateColliderBufferSize(numFound);
                numFound = Physics.OverlapBoxNonAlloc(
                    position + rotation * m_BoundsCenter,
                    m_BoundsExtents, 
                    m_ColliderBuffer, 
                    rotation, 
                    m_LayerMask);
            }
            while (numFound == m_ColliderBufferSize);

            ParseColliders(numFound, m_Constraint, worldToLocalMatrix);
        }
    }

    /// <summary>
    /// Constraint for 2D detection.
    /// </summary>
    public class DetectionConstraint2D : DetectionConstraint
    {
        /// <summary>
        /// Detection bounds.
        /// </summary>
        public Bounds Bounds
        {
            set
            {
                m_Bounds = value;
                m_BoundRect = new Rect(value.min.x, value.min.z, value.size.x, value.size.z);
            }
            get { return m_Bounds; }
        }
        private Bounds m_Bounds;
        private Rect m_BoundRect;

        /// <inheritdoc/>
        public override bool ContainsPoint(Vector3 localPoint, out Vector3 normPoint)
        {
            if (m_Bounds.Contains(localPoint))
            {
                normPoint = Rect.PointToNormalized(m_BoundRect, new Vector2(localPoint.x, localPoint.z));
                return true;
            }

            normPoint = default;
            return false;
        }
    }
}
