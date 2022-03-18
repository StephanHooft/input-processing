using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store twin (x,y) <see cref="Axis"/> values such as those of a control stick.
    /// </summary>
    public class TwinAxes
    {
        #region Events

        /// <summary>
        /// Invoked when the <see cref="TwinAxes"/> is set.
        /// </summary>
        public event System.Action<TwinAxes> OnValueChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Properties

        /// <summary>
        /// The X-<see cref="Axis"/> component of the <see cref="TwinAxes"/>.
        /// </summary>
        public Axis AxisX => axisX;

        /// <summary>
        /// The Y-<see cref="Axis"/> component of the <see cref="TwinAxes"/>.
        /// </summary>
        public Axis AxisY => axisY;

        /// <summary>
        /// <see cref="true"/> if <see cref="AxisX"/> and <see cref="AxisY"/> are both neutral (0f).
        /// <para>Will always be <see cref="false"/> if the <see cref="TwinAxes"/> was never set.</para>
        /// </summary>
        public bool Neutral => AxisX.Neutral && AxisY.Neutral;

        /// <summary>
        /// The value of the <see cref="TwinAxes"/> represented as a <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Value => new Vector2(AxisX.Value, AxisY.Value);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Fields

        private InputAction action;
        private readonly Axis 
            axisX,
            axisY;
        private readonly System.Action<float, bool>
            setX, 
            setY;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Constructors and Finaliser

        /// <summary>
        /// Creates a new <see cref="TwinAxes"/>.
        /// </summary>
        public TwinAxes()
        {
            axisX = new Axis(out setX);
            axisY = new Axis(out setY);
            axisX.OnValueChanged += AxisValueChanged;
            axisY.OnValueChanged += AxisValueChanged;
        }

        /// <summary>
        /// Creates a new <see cref="TwinAxes"/> and ties it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="TwinAxes"/>
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="action"/>'s control type is invalid.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="action"/> is <see cref="null"/>.
        /// </exception>
        public TwinAxes(InputAction action)
        {
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            if (action.expectedControlType != "Vector2")
                throw
                    new System.ArgumentException(InvalidControlFormat(action));
            axisX = new Axis(out setX);
            axisY = new Axis(out setY);
            axisX.OnValueChanged += AxisValueChanged;
            axisY.OnValueChanged += AxisValueChanged;
            SetInputAction(action);
        }

        /// <summary>
        /// Finaliser.
        /// </summary>
        ~TwinAxes()
        {
            if (action != null)
            {
                action.performed -= ActionPerformed;
                action.canceled -= ActionCanceled;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Conversion Methods

        /// <summary>
        /// A <see cref="TwinAxes"/> can implicitly be converted to its <see cref="Vector2"/> value.
        /// </summary>
        public static implicit operator Vector2(TwinAxes twinAxes)
            => twinAxes.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Methods

        /// <summary>
        /// Returns the angle of the <see cref="TwinAxes"/>, as compared to <see cref="Vector2.up"/>.
        /// </summary>
        public float Angle()
            => Vector2.Angle(Vector2.up, Value);

        /// <summary>
        /// The angle of the <see cref="TwinAxes"/>, as compared to an<paramref name="other"/> <see cref="Vector2"/>.
        /// </summary>
        public float Angle(Vector2 other)
            => Vector2.Angle(other, Value);

        /// <summary>
        /// Ties the <see cref="TwinAxes"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="TwinAxes"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="action"/>'s control type is invalid.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="action"/> is <see cref="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If another <see cref="InputAction"/> is already registered to the <see cref="TwinAxes"/>.
        /// </exception>
        public void RegisterInputAction(InputAction action)
        {
            if (this.action != null)
                throw
                    new System.InvalidOperationException(ActionAlreadyRegistered);
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            if (action.expectedControlType != "Vector2")
                throw
                    new System.ArgumentException(InvalidControlFormat(action));
            SetInputAction(action);
        }

        /// <summary>
        /// Sets the <see cref="TwinAxes"/>. This is only permitted if no <see cref="InputAction"/> was registered to
        /// the <see cref="TwinAxes"/>.
        /// </summary>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// If a <see cref="InputAction"/> is registered to the <see cref="TwinAxes"/>.
        /// </exception>
        public void SetValue(Vector2 value)
        {
            if (action != null)
                throw
                    new System.InvalidOperationException(CannotSetValueWhenActionSet);
            axisX.SetValue(value.x, false); // The Axes' update events are not invoked
            axisY.SetValue(value.y, false);
            OnValueChanged?.Invoke(this);
        }

        /// <summary>
        /// Unsets a registered <see cref="InputAction"/>, if any.
        /// </summary>
        public void UnregisterInputAction()
        {
            if (action != null)
            {
                action.performed -= ActionPerformed;
                action.canceled -= ActionCanceled;
            }
            action = null;
        }

        private void AxisValueChanged(InputProcessing.Axis axis)
            => OnValueChanged?.Invoke(this);

        private void ActionCanceled(InputAction.CallbackContext context)
            => ActionSetValue(Vector2.zero);

        private void ActionPerformed(InputAction.CallbackContext context)
            => ActionSetValue(context.ReadValue<Vector2>());

        private void ActionSetValue(Vector2 value)
        {
            setX.Invoke(value.x, false);
            setY.Invoke(value.y, false);
            OnValueChanged?.Invoke(this);
        }

        private void SetInputAction(InputAction action)
        {
            action.performed += ActionPerformed;
            action.canceled += ActionCanceled;
            this.action = action;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Axis Class

        /// <summary>
        /// Class to process/store an axial <see cref="float"/> value such as a trigger/stick position.
        /// <para>This custom <see cref="TwinAxes"/> version of the <see cref="InputProcessing.Axis"/> ensures that
        /// the <see cref="TwinAxes"/> is the only one that can set its value.</para>
        /// </summary>
        public class Axis : InputProcessing.Axis
        {
            /// <summary>
            /// An <see cref="Axis"/> can implicitly be converted to its <see cref="float"/> value.
            /// </summary>
            public static implicit operator float(Axis axis)
                => axis.Value;

            /// <summary>
            /// Creates a new <see cref="Axis"/>. Used by a <see cref="TwinAxes"/> to create the <see cref="Axis"/>.
            /// </summary>
            /// <param name="setMethod">
            /// The callback method that must be used to set the <see cref="Axis"/>.
            /// </param>
            public Axis(out System.Action<float, bool> setMethod)
                => setMethod = SetFromTwinAxes;

            /// <summary>
            /// This method may not be called.
            /// </summary>
            /// <exception cref="System.InvalidOperationException">
            /// As soon as the method is called.
            /// </exception>
            public override void RegisterInputAction(InputAction action)
                => throw
                    new System.InvalidOperationException(NotPermitted);

            /// <summary>
            /// This method may not be called.
            /// </summary>
            /// <exception cref="System.InvalidOperationException">
            /// As soon as the method is called.
            /// </exception>
            public override void SetValue(float value, bool invoke = true)
                => throw
                    new System.InvalidOperationException(NotPermitted);

            /// <summary>
            /// This method may not be called.
            /// </summary>
            /// <exception cref="System.InvalidOperationException">
            /// As soon as the method is called.
            /// </exception>
            public override void UnregisterInputAction()
                => throw
                    new System.InvalidOperationException(NotPermitted);

            private void SetFromTwinAxes(float value, bool invoke = true)
                => UpdateValue(value, invoke);

            private static string NotPermitted => string.Format(
                "This method may not be called on a {0}.",
                typeof(Axis).FullName);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Error Messages

        private static string ActionAlreadyRegistered => string.Format(
            "An {0} has already been registered to the {1}.",
            typeof(InputAction).Name, typeof(TwinAxes).Name);
        private static string CannotSetValueWhenActionSet => string.Format(
            "Cannot set {0} value while an {1} has been registered to it.",
            typeof(TwinAxes).Name, typeof(InputAction).Name);

        private static string InvalidControlFormat(InputAction action) => string.Format(
            "{0} {1} does not have an expected {2} control type.",
            typeof(InputAction).Name, action.name, typeof(Vector2).Name);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
