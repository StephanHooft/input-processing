using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store an axial <see cref="float"/> value such as a trigger/stick position.
    /// </summary>
    public class Axis
    {
        #region Events

        /// <summary>
        /// Invoked when the <see cref="Axis"/> shifts to a neutral value (0f).
        /// </summary>
        public event System.Action<Axis> OnNeutral;

        /// <summary>
        /// Invoked when the <see cref="Axis"/> shifts to a negative value (less than 0f).
        /// </summary>
        public event System.Action<Axis> OnNegative;

        /// <summary>
        /// Invoked when the <see cref="Axis"/> shifts to a positive value (greater than 0f).
        /// </summary>
        public event System.Action<Axis> OnPositive;

        /// <summary>
        /// Invoked when the <see cref="Axis"/> is set.
        /// </summary>
        public event System.Action<Axis> OnValueChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Properties

        /// <summary>
        /// <see cref="true"/> if the <see cref="Axis"/> is negative (less than 0f).
        /// </summary>
        public bool Negative => set && Value < 0f;

        /// <summary>
        /// <see cref="true"/> if the <see cref="Axis"/> is neutral (0f).
        /// </summary>
        public bool Neutral => set && Value == 0f;

        /// <summary>
        /// <see cref="true"/> if the <see cref="Axis"/> is positive (greater than 0f).
        /// </summary>
        public bool Positive => set && Value > 0f;

        /// <summary>
        /// The current value of the <see cref="Axis"/>.
        /// </summary>
        public float Value { get; private set; } = 0f;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Fields

        private InputAction action;
        private int frame;
        private float time;
        private bool set = false;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Constructors and Finaliser

        /// <summary>
        /// Creates a new <see cref="Axis"/>.
        /// </summary>
        public Axis()
        { }

        /// <summary>
        /// Creates a new <see cref="Axis"/> and ties it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="Axis"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="action"/> is <see cref="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="action"/>'s control type is invalid.
        /// </exception>
        public Axis(InputAction action)
        {
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            if (action.expectedControlType != "Axis")
                throw
                    new System.ArgumentException(InvalidControlFormat(action));
            SetInputAction(action);
        }

        /// <summary>
        /// Finaliser.
        /// </summary>
        ~Axis()
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
        /// An <see cref="Axis"/> can implicitly be converted to its <see cref="float"/> value.
        /// </summary>
        public static implicit operator float(Axis axis) => axis.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Methods

        /// <summary>
        /// Checks whether the <see cref="Axis"/> is above a certain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to compare the <see cref="Axis"/> with.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is above <paramref name="value"/>.
        /// </returns>
        public bool Above(float value)
            => Value > value;

        /// <summary>
        /// Checks whether the <see cref="Axis"/> is above or equal to a certain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to compare the <see cref="Axis"/> with.
        /// </param>
        /// <returns><see cref="true"/> if the <see cref="Axis"/> is above or equal to <paramref name="value"/>.
        /// </returns>
        public bool AboveOrEqualTo(float value)
            => Value >= value;

        /// <summary>
        /// Checks whether the <see cref="Axis"/> is below a certain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to compare the <see cref="Axis"/> with.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is below <paramref name="value"/>.
        /// </returns>
        public bool Below(float value)
            => Value < value;

        /// <summary>
        /// Checks whether the <see cref="Axis"/> is below or equal to a certain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to compare the <see cref="Axis"/> with.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is below or equal to <paramref name="value"/>.
        /// </returns>
        public bool BelowOrEqualTo(float value)
            => Value <= value;

        /// <summary>
        /// Checks whether the <see cref="Axis"/> is equal to a certain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to compare the <see cref="Axis"/> with.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is equal to <paramref name="value"/>.
        /// </returns>
        public bool EqualTo(float value)
            => Value == value;

        /// <summary>
        /// Calculates the duration (in frames) for which the <see cref="Axis"/> has been positive.
        /// </summary>
        /// <returns>
        /// How many update frames the <see cref="Axis"/> has been positive, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not positive.</para>
        /// </returns>
        public int GetPositiveDurationFrames()
            => Positive ? Time.frameCount - frame : -1;

        /// <summary>
        /// Calculates the duration (in seconds) for which the <see cref="Axis"/> has been positive.
        /// </summary>
        /// <returns>
        /// How many seconds the <see cref="Axis"/> has been positive, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not positive.</para>
        /// </returns>
        public float GetPositiveDurationSeconds()
            => Positive ? Time.unscaledTime - time : -1f;

        /// <summary>
        /// Calculates the duration (in frames) for which the <see cref="Axis"/> has been neutral (0f).
        /// </summary>
        /// <returns>
        /// How many update frames the <see cref="Axis"/> has been neutral, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not neutral.</para>
        /// </returns>
        public int GetNeutralDurationFrames()
            => Neutral ? Time.frameCount - frame : -1;

        /// <summary>
        /// Calculates the duration (in seconds) for which the <see cref="Axis"/> has been neutral (0f).
        /// </summary>
        /// <returns>
        /// How many seconds the <see cref="Axis"/> has been neutral, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not neutral.</para>
        /// </returns>
        public float GetNeutralDurationSeconds()
            => Neutral ? Time.unscaledTime - time : -1f;

        /// <summary>
        /// Calculates the duration (in frames) for which the <see cref="Axis"/> has been negative.
        /// </summary>
        /// <returns>
        /// How many update frames the <see cref="Axis"/> has been negative, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not negative.</para>
        /// </returns>
        public int GetNegativeDurationFrames()
            => Negative ? Time.frameCount - frame : -1;

        /// <summary>
        /// Calculates the duraction (in seconds) for which the <see cref="Axis"/> has been negative.
        /// </summary>
        /// <returns>
        /// How many seconds the <see cref="Axis"/> has been negative, if it currently is.
        /// <para>-1 will be returned if the <see cref="Axis"/> is currently not negative.</para>
        /// </returns>
        public float GetNegativeDurationSeconds()
            => Negative ? Time.unscaledTime - time : -1f;

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been negative for a certain <paramref name="duration"/> in frames.
        /// </summary>
        /// <param name="duration">
        /// The duration (in frames) that the <see cref="Axis"/> should have been negative for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is negative, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool NegativeForFrames(int duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("durationFrames", ValueMustNotBeNegative("durationFrames"));
            return
                Negative && duration >= frame;
        }

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been negative for a certain <paramref name="duration"/> in
        /// seconds.
        /// </summary>
        /// <param name="duration">
        /// The duration (in seconds) that the <see cref="Axis"/> should have been negative for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is negative, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool NegativeForSeconds(float duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("duration", ValueMustNotBeNegative("duration"));
            return
                Negative && duration >= time;
        }

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been neutral for a certain <paramref name="duration"/> in frames.
        /// </summary>
        /// <param name="duration">
        /// The duration (in frames) that the <see cref="Axis"/> should have been neutral for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is neutral, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool NeutralForFrames(int duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("durationFrames", ValueMustNotBeNegative("durationFrames"));
            return
                Neutral && duration >= frame;
        }

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been neutral for a certain <paramref name="duration"/> in seconds.
        /// </summary>
        /// <param name="duration">
        /// The duration (in seconds) that the <see cref="Axis"/> should have been neutral for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is neutral, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool NeutralForSeconds(float duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("durationSeconds",
                    ValueMustNotBeNegative("durationSeconds"));
            return
                Neutral && duration >= time;
        }

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been positive for a certain <paramref name="duration"/> in frames.
        /// </summary>
        /// <param name="duration">
        /// The duration (in frames) that the <see cref="Axis"/> should have been positive for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is positive, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool PositiveForFrames(int duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("durationFrames", ValueMustNotBeNegative("durationFrames"));
            return
                Positive && duration >= frame;
        }

        /// <summary>
        /// Checks whether the <see cref="Axis"/> has been positive for a certain <paramref name="duration"/> in
        /// seconds.
        /// </summary>
        /// <param name="duration">
        /// The duration (in seconds) that the <see cref="Axis"/> should have been positive for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Axis"/> is positive, and has been so for the
        /// <paramref name="duration"/> or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool PositiveForSeconds(float duration)
        {
            if (duration < 0f)
                throw
                    new System.ArgumentOutOfRangeException("durationSeconds",
                    ValueMustNotBeNegative("durationSeconds"));
            return
                Positive && duration >= time;
        }

        /// <summary>
        /// Ties the <see cref="Axis"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="Axis"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="action"/>'s control type is invalid.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="action"/> is <see cref="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If another <see cref="InputAction"/> is already registered to the <see cref="Axis"/>.
        /// </exception>
        public virtual void RegisterInputAction(InputAction action)
        {
            if (this.action != null)
                throw
                    new System.InvalidOperationException(ActionAlreadyRegistered);
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            if (action.expectedControlType != "Axis")
                throw
                    new System.ArgumentException(InvalidControlFormat(action));
            SetInputAction(action);
        }

        /// <summary>
        /// Sets the <see cref="Axis"/>. This is only permitted if no <see cref="InputAction"/> was registered to the
        /// <see cref="Axis"/>.
        /// <para>The <see cref="Axis"/> logs <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> 
        /// whenever it is set.</para>
        /// </summary>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <param name="invoke">
        /// Set to <see cref="false"/> to prevent the <see cref="OnValueChanged"/>' from being
        /// invoked by this method call.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// 
        /// </exception>
        public virtual void SetValue(float value, bool invoke = true)
        {
            if (action != null)
                throw
                    new System.InvalidOperationException(CannotSetValueWhenActionSet);
            UpdateValue(value, invoke);
        }

        /// <summary>
        /// Unsets a registered <see cref="InputAction"/>, if any.
        /// </summary>
        public virtual void UnregisterInputAction()
        {
            if (action != null)
            {
                action.performed -= ActionPerformed;
                action.canceled -= ActionCanceled;
            }
            action = null;
        }

        /// <summary>
        /// Updates the <see cref="Axis"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="float"/> value to set.
        /// </param>
        /// <param name="invoke">
        /// Whether or not to invoke the <see cref="Axis"/> update events.
        /// </param>
        protected void UpdateValue(float value, bool invoke)
        {
            if (value != Value)
            {
                if (value > 0f)
                {
                    if (!Positive)
                    {
                        frame = Time.frameCount;
                        time = Time.unscaledTime;
                        if (invoke)
                            OnPositive?.Invoke(this);
                    }
                }
                else if (value < 0f)
                {
                    if (!Negative)
                    {
                        frame = Time.frameCount;
                        time = Time.unscaledTime;
                        if (invoke)
                            OnNegative?.Invoke(this);
                    }
                }
                else
                {
                    if (!Neutral)
                    {
                        frame = Time.frameCount;
                        time = Time.unscaledTime;
                        if (invoke)
                            OnNeutral?.Invoke(this);
                    }
                }
                Value = value;
                if (!set)
                    set = true;
                if (invoke)
                    OnValueChanged?.Invoke(this);
            }
        }

        private void ActionCanceled(InputAction.CallbackContext context)
        {
            ActionSetValue(0f);
        }

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            ActionSetValue(context.ReadValue<float>());
        }

        private void ActionSetValue(float value)
        {
            UpdateValue(value, true);
        }

        private void SetInputAction(InputAction action)
        {
            action.performed += ActionPerformed;
            action.canceled += ActionCanceled;
            this.action = action;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Error Messages

        private static string ActionAlreadyRegistered => string.Format(
            "An {0} has already been registered to the {1}.",
            typeof(InputAction).Name, typeof(TwinAxes).Name);

        private static string CannotSetValueWhenActionSet => string.Format(
            "Cannot set {0} value while an {1} has been registered to it.",
            typeof(Axis).Name, typeof(InputAction).Name);

        private static string InvalidControlFormat(InputAction action) => string.Format(
            "{0} {1} does not have an expected {2} control type.",
            typeof(InputAction).Name, action.name, "Axis");

        private static string ValueMustNotBeNegative(string valueName) => string.Format(
            "{0} value must not be negative.",
            valueName);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
