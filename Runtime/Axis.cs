using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store axial values such as trigger/stick positions.
    /// </summary>
    public class Axis
    {
        /// <summary>
        /// The current value of the <see cref="Axis"/>.
        /// </summary>
        public float Value { get; private set; } = 0f;

        /// <summary>
        /// True if <see cref="Value"/> is positive (greater than 0f).\
        /// <para>Will always be false if <see cref="SetValue"/> has never been called.</para>
        /// </summary>
        public bool Positive => set && Value > 0f;

        /// <summary>
        /// True if <see cref="Value"/> is neutral (0f).
        /// <para>Will always be false if <see cref="SetValue"/> has never been called.</para>
        /// </summary>
        public bool Neutral => set && Value == 0f;

        /// <summary>
        /// True if <see cref="Value"/> is negative (less than 0f).
        /// <para>Will always be false if <see cref="SetValue"/> has never been called.</para>
        /// </summary>
        public bool Negative => set && Value < 0f;

        /// <summary>
        /// Invoked when <see cref="Value"/> is set..
        /// </summary>
        public event System.Action<float> OnValueChanged;

        /// <summary>
        /// Invoked when <see cref="Value"/> shifts to a positive value (greater than 0f).
        /// </summary>
        public event System.Action OnPositive;

        /// <summary>
        /// Invoked when <see cref="Value"/> shifts to a negative value (less than 0f).
        /// </summary>
        public event System.Action OnNegative;

        /// <summary>
        /// Invoked when <see cref="Value"/> shifts to a neutral value (0f).
        /// </summary>
        public event System.Action OnNeutral;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int positiveFrame = 0;
        private int neutralFrame = 0;
        private int negativeFrame = 0;
        private float positiveTime = 0f;
        private float neutralTime = 0f;
        private float negativeTime = 0f;
        private bool set = false;
        private InputAction action;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new <see cref="Axis"/>.
        /// </summary>
        public Axis()
        {}

        /// <summary>
        /// Create a new <see cref="Axis"/> and tie it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="Axis"/>.</param>
        public Axis(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (action.expectedControlType != "Axis")
                throw new System.ArgumentException(string.Format("InputAction {0} does not have an expected Axis control type.", action.name));
            SetInputAction(action);
        }

        ~Axis()
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
        /// <para>The <see cref="Axis"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> 
        /// whenever <see cref="Value"/> is set.</para>
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="invoke">Set to false to prevent the class' events from being invoked by this method.
        /// <para><see cref="TwinAxes"/> relies on <see cref="OnValueChanged"/> to monitor its instances of <see cref="Axis"/> 
        /// for value changes, so take care.</para></param>
        public void SetValue(float value, bool invoke = true)
        {
            if(value != Value)
            {
                if (value > 0f)
                {
                    if (!Positive)
                    {
                        positiveFrame = Time.frameCount;
                        positiveTime = Time.unscaledTime;
                        if(invoke)
                            OnPositive?.Invoke();
                    }
                }
                else if (value < 0f)
                {
                    if (!Negative)
                    {
                        negativeFrame = Time.frameCount;
                        negativeTime = Time.unscaledTime;
                        if (invoke)
                            OnNegative?.Invoke();
                    }
                }
                else
                {
                    if (!Neutral)
                    {
                        neutralFrame = Time.frameCount;
                        neutralTime = Time.unscaledTime;
                        if (invoke)
                            OnNeutral?.Invoke();
                    }
                }
                Value = value;
                if (!set)
                    set = true;
                if (invoke)
                    OnValueChanged?.Invoke(Value);
            }
        }

        /// <summary>
        /// Calculates the amount of update frames <see cref="Value"/> has been positive for.
        /// </summary>
        /// <returns>
        /// How many update frames <see cref="Value"/> has been positive, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not positive, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public int GetPositiveDurationFrames()
        {
            if (set && Positive && positiveFrame >= negativeFrame && positiveFrame >= neutralFrame)
                return Time.frameCount - positiveFrame;
            else
                return -1;
        }

        /// <summary>
        /// Calculates the amount of seconds <see cref="Value"/> has been positive for.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been positive, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not positive, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public float GetPositiveDurationTime()
        {
            if (set && Positive && positiveTime >= negativeTime && positiveTime >= neutralTime)
                return Time.unscaledTime - positiveTime;
            else
                return -1f;
        }

        /// <summary>
        /// Calculates the amount of update frames <see cref="Value"/> has been neutral (0f) for.
        /// </summary>
        /// <returns>
        /// How many update frames <see cref="Value"/> has been neutral, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not neutral, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public int GetNeutralDurationFrames()
        {
            if (set && Neutral && neutralFrame >= negativeFrame && neutralFrame >= positiveFrame)
                return Time.frameCount - neutralFrame;
            else
                return -1;
        }

        /// <summary>
        /// Calculates the amount of  seconds <see cref="Value"/> has been neutral (0f) for.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been neutral, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not neutral, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public float GetNeutralDurationTime()
        {
            if (set && Neutral && neutralTime >= negativeTime && neutralTime >= positiveTime)
                    return Time.unscaledTime - neutralTime;
            else
                return -1f;
        }

        /// <summary>
        /// Calculates the amount of update frames <see cref="Value"/> has been negative for.
        /// </summary>
        /// <returns>
        /// How many update frames <see cref="Value"/> has been negative, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not negative, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public int GetNegativeDurationFrames()
        {
            if (set && Negative && negativeFrame >= positiveFrame && negativeFrame >= neutralFrame)
                return Time.frameCount - negativeFrame;
            else
                return -1;
        }

        /// <summary>
        /// Calculates the amount of seconds <see cref="Value"/> has been negative for.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been negative, if it currently is.
        /// <para>-1 will be returned if <see cref="Value"/> is currently not negative, or if <see cref="SetValue"/> has never been called.</para>
        /// </returns>
        public float GetNegativeDurationTime()
        {
            if (set && Negative && negativeTime >= positiveTime && negativeTime >= neutralTime)
                return Time.unscaledTime - neutralTime;
            else
                return -1f;
        }

        /// <summary>
        /// Tie the <see cref="Axis"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="Axis"/>.</param>
        public void RegisterInputAction(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (action.expectedControlType != "Axis")
                throw new System.ArgumentException(string.Format("InputAction {0} does not have an expected Axis control type.", action.name));
            SetInputAction(action);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            SetValue(context.ReadValue<float>());
        }

        private void ActionCanceled(InputAction.CallbackContext context)
        {
            SetValue(0f);
        }

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
    }
}
