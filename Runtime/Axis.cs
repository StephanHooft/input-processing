using UnityEngine;
using UnityEngine.InputSystem;

namespace InputProcessing
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
        /// <para>Will always be false if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.</para>
        /// </summary>
        public bool Positive => set && Value > 0f;

        /// <summary>
        /// True if <see cref="Value"/> is neutral (0f).
        /// <para>Will always be false if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.</para>
        /// </summary>
        public bool Neutral => set && Value == 0f;

        /// <summary>
        /// True if <see cref="Value"/> is negative (less than 0f).
        /// <para>Will always be false if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.</para>
        /// </summary>
        public bool Negative => set && Value < 0f;

        /// <summary>
        /// Invoked when <see cref="Value"/> is set through <see cref="SetValue(float)"/> or <see cref="SetNeutral"/>.
        /// </summary>
        public event System.Action<float> OnValueChanged;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int positiveFrame = 0;
        private int neutralFrame = 0;
        private int negativeFrame = 0;
        private float positiveTime = 0f;
        private float neutralTime = 0f;
        private float negativeTime = 0f;
        private bool set = false;
        private readonly InputAction action;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new <see cref="Axis"/>.
        /// </summary>
        public Axis()
        { }

        /// <summary>
        /// Create a new <see cref="Axis"/> and tie it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="Axis"/>.</param>
        public Axis(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (action.expectedControlType != "Axis")
                throw new System.ArgumentException("InputAction " + action.name + " does not have an expected Axis control type.");
            action.performed += ActionPerformed;
            action.canceled += ActionCanceled;
            this.action = action;
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
        /// Set <see cref="Value"/>.
        /// <para>The <see cref="Axis"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever <see cref="Value"/> is set.</para>
        /// </summary>
        /// <param name="value">The value to set <see cref="Value"/> to.</param>
        public void SetValue(float value)
        {
            SetValue(value, true);
        }

        /// <summary>
        /// Set the <see cref="Axis"/> value.
        /// <para>The <see cref="Axis"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever <see cref="Value"/> is set.</para>
        /// <para>It is strongly recommended not to set the <paramref name="invoke"/> parameter to false, unless you know what you are doing. 
        /// Other classes such as <see cref="TwinAxes"/> use the <see cref="OnValueChanged"/> event to monitor their instances of <see cref="Axis"/> for value changes.</para>
        /// </summary>
        /// <param name="value">The value to set <see cref="Value"/> to.</param>
        /// <param name="invoke">Invokes <see cref="OnValueChanged"/> if true.</param>
        public void SetValue(float value, bool invoke = true)
        {
            if (value > 0f)
            {
                if (!Positive)
                {
                    positiveFrame = Time.frameCount;
                    positiveTime = Time.unscaledTime;
                }
            }
            else if (value < 0f)
            {
                if (!Negative)
                {
                    negativeFrame = Time.frameCount;
                    negativeTime = Time.unscaledTime;
                }
            }
            else
            {
                if (!Neutral)
                {
                    neutralFrame = Time.frameCount;
                    neutralTime = Time.unscaledTime;
                }
            }
            Value = value;
            if (!set) 
                set = true;
            if (invoke) 
                OnValueChanged?.Invoke(Value);
        }

        /// <summary>
        /// Set <see cref="Value"/> to neutral (0f).
        /// <para>The <see cref="Axis"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever <see cref="Value"/> is set.</para>
        /// </summary>
        public void SetNeutral()
        {
            SetValue(0f);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns for how many frames <see cref="Value"/> has been positive, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns>
        /// How many frames <see cref="Value"/> has been positive, if it currently is. If <see cref="Value"/> is currently not positive, this method will return -1.
        /// This method will also always return -1 if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public int PositiveDurationFrames()
        {
            if (set && Positive && positiveFrame >= negativeFrame && positiveFrame >= neutralFrame)
                return Time.frameCount - positiveFrame;

            else return -1;
        }

        /// <summary>
        /// Returns for how many seconds <see cref="Value"/> has been positive, if it currently is. Returns -1f otherwise.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been positive, if it currently is. If <see cref="Value"/> is currently not positive, this method will return -1f.
        /// This method will also always return -1f if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public float PositiveDurationTime()
        {
            if (set && Positive && positiveTime >= negativeTime && positiveTime >= neutralTime)
                return Time.unscaledTime - positiveTime;

            else return -1f;
        }

        /// <summary>
        /// Returns for how many frames <see cref="Value"/> has been neutral (0f), if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns>
        /// How many frames <see cref="Value"/> has been neutral, if it currently is. If <see cref="Value"/> is currently not neutral, this method will return -1.
        /// This method will also always return -1 if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public int NeutralDurationFrames()
        {
            if (set && Neutral && neutralFrame >= negativeFrame && neutralFrame >= positiveFrame)
                return Time.frameCount - neutralFrame;

            else return -1;
        }

        /// <summary>
        /// Returns for how many seconds <see cref="Value"/> has been neutral (0f), if it currently is. Returns -1f otherwise.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been neutral, if it currently is. If <see cref="Value"/> is currently not neutral, this method will return -1f.
        /// This method will also always return -1f if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public float NeutralDurationTime()
        {
            if (set && Neutral && neutralTime >= negativeTime && neutralTime >= positiveTime)
                return Time.unscaledTime - neutralTime;

            else return -1f;
        }

        /// <summary>
        /// Returns for how many frames <see cref="Value"/> has been negative, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns>
        /// How many frames <see cref="Value"/> has been negative, if it currently is. If <see cref="Value"/> is currently not negative, this method will return -1.
        /// This method will also always return -1 if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public int NegativeDurationFrames()
        {
            if (set && Negative && negativeFrame >= positiveFrame && negativeFrame >= neutralFrame)
                return Time.frameCount - negativeFrame;

            else return -1;
        }

        /// <summary>
        /// Returns for how many seconds <see cref="Value"/> has been negative, if it currently is. Returns -1f otherwise.
        /// </summary>
        /// <returns>
        /// How many seconds <see cref="Value"/> has been negative, if it currently is. If <see cref="Value"/> is currently not negative, this method will return -1f.
        /// This method will also always return -1f if <see cref="SetValue(float)"/> or <see cref="SetNeutral"/> have never been called.
        /// </returns>
        public float NegativeDurationTime()
        {
            if (set && Negative && negativeTime >= positiveTime && negativeTime >= neutralTime)
                return Time.unscaledTime - neutralTime;

            else return -1f;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            SetValue(context.ReadValue<float>());
        }

        private void ActionCanceled(InputAction.CallbackContext context)
        {
            SetNeutral();
        }
    }
}
