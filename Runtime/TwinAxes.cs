using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store twin (x,y) <see cref="Axis"/> values such as those of a control stick.
    /// </summary>
    public class TwinAxes
    {
        /// <summary>
        /// The value of the <see cref="TwinAxes"/> represented as a <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Value => new Vector2(AxisX.Value, AxisY.Value);

        /// <summary>
        /// The angle of <see cref="Value"/>, as compared to <see cref="Vector2.up"/>.
        /// </summary>
        public float Angle => Vector2.Angle(Vector2.up, Value);

        /// <summary>
        /// The X-<see cref="Axis"/> component of the <see cref="TwinAxes"/>.
        /// </summary>
        public Axis AxisX { get; private set; } = new Axis();

        /// <summary>
        /// The Y-<see cref="Axis"/> component of the <see cref="TwinAxes"/>.
        /// </summary>
        public Axis AxisY { get; private set; } = new Axis();

        /// <summary>
        /// True if the <see cref="Axis.Value"/> of <see cref="AxisX"/> and <see cref="AxisY"/> are both neutral (0f).
        /// <para>Will always be false if <see cref="SetValue(Vector2)"/> has never been called.</para>
        /// </summary>
        public bool Neutral => AxisX.Neutral && AxisY.Neutral;

        /// <summary>
        /// Invoked when <see cref="Value"/> is set through <see cref="SetValue(Vector2)"/>.
        /// </summary>
        public event System.Action<Vector2> OnValueChanged;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private InputAction action;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new <see cref="TwinAxes"/>.
        /// </summary>
        public TwinAxes()
        {}

        /// <summary>
        /// Create a new <see cref="TwinAxes"/> and tie it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="TwinAxes"/></param>
        public TwinAxes(InputAction action)
        {
            if (action == null) 
                throw new System.ArgumentNullException("action");
            if (action.expectedControlType != "Vector2")
                throw new System.ArgumentException("InputAction " + action.name + " does not have an expected Vector2 control type.");
            SetInputAction(action);
        }

        ~TwinAxes()
        {
            if (action != null)
            {
                action.performed -= ActionPerformed;
                action.canceled -= ActionCanceled;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets <see cref="Value"/>.
        /// <para>Both <see cref="Axis"/> of the <see cref="TwinAxes"/> log the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> 
        /// whenever <see cref="Value"/> is set.</para>
        /// </summary>
        /// <param name="value">The value to set <see cref="Value"/> to.</param>
        public void SetValue(Vector2 value)
        {
            // The Axes' update events are not invoked, because TwinAxes is the one setting their value.
            AxisX.SetValue(value.x, false);
            AxisY.SetValue(value.y, false);
            OnValueChanged?.Invoke(Value);
        }

        /// <summary>
        /// Tie the <see cref="TwinAxes"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="TwinAxes"/>.</param>
        public void RegisterInputAction(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (action.expectedControlType != "Vector2")
                throw new System.ArgumentException("InputAction " + action.name + " does not have an expected Vector2 control type.");
            SetInputAction(action);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void SetInputAction(InputAction action)
        {
            if (this.action != null)
            {
                this.action.performed -= ActionPerformed;
                this.action.canceled -= ActionCanceled;
            }
            action.performed += ActionPerformed;
            action.canceled += ActionCanceled;
            this.action = action;
        }

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            SetValue(context.ReadValue<Vector2>());
        }

        private void ActionCanceled(InputAction.CallbackContext context)
        {
            SetValue(Vector2.zero);
        }
    }
}
