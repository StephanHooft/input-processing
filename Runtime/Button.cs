using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store button presses/releases and other binary inputs.
    /// <para><see cref="Button"/> presses/releases must be "accepted" to prevent the <see cref="Button"/> from
    /// returning duplicate/false positives.</para>
    /// </summary>
    public class Button
    {
        #region Events

        /// <summary>
        /// Invoked when the <see cref="Button"/> is pressed.
        /// </summary>
        public event System.Action<Button> OnPressed;

        /// <summary>
        /// Invoked when the <see cref="Button"/> is released.
        /// </summary>
        public event System.Action<Button> OnReleased;

        /// <summary>
        /// Invoked when the <see cref="Button"/> is set through either <see cref="Press"/>, <see cref="Release"/>,
        /// or <see cref="Toggle"/>.
        /// </summary>
        public event System.Action<Button> OnValueChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Properties

        /// <summary>
        /// The current value of the <see cref="Button"/>. <see cref="true"/> means pressed, whereas <see cref="false"/>
        /// means released.
        /// </summary>
        public bool Value { get; private set; } = false;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Fields

        private bool
            pressAvailable = false,
            releaseAvailable = false,
            set = false;
        private int
            pressFrame = 0,
            releaseFrame = 0;
        private float
            pressTime = 0f,
            releaseTime = 0f;
        private InputAction action;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Constructors and Finaliser

        /// <summary>
        /// Creates a new <see cref="Button"/>.
        /// </summary>
        public Button()
        { }

        /// <summary>
        /// Creates a new <see cref="Button"/> and ties it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="Button"/>.
        /// </param>
        public Button(InputAction action)
        {
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            SetInputAction(action);
        }

        ~Button()
        {
            if (action != null)
            {
                action.performed -= context => Press();
                action.canceled -= context => Release();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Conversion Methods

        /// <summary>
        /// A <see cref="Button"/> can implicitly be converted to its <see cref="bool"/> value.
        /// </summary>
        public static implicit operator bool(Button button) => button.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Methods

        /// <summary>
        /// Accepts the current <see cref="Button"/> value. This will prevent the various "Pressed" and "Released"
        /// methods from returning <see cref="true"/> until the next time the <see cref="Button"/> value changes.
        /// </summary>
        public void AcceptValue()
        {
            pressAvailable = false;
            releaseAvailable = false;
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been pressed, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns></returns>
        public int GetPressDurationFrames()
        {
            return
                set && Value && pressFrame >= releaseFrame
                ? Time.frameCount - pressFrame
                : -1;
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been pressed, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns></returns>
        public float GetPressDurationSeconds()
        {
            return
                set && Value && pressTime >= releaseTime
                ? Time.unscaledTime - pressTime
                : -1f;
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been released, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns></returns>
        public int GetReleasedDurationFrames()
        {
            return
                set && !Value && releaseFrame >= pressFrame
                ? Time.frameCount - releaseFrame
                : -1;
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been released, if it currently is. Returns -1 otherwise.
        /// </summary>
        /// <returns></returns>
        public float GetReleasedDurationSeconds()
        {
            return
                set && !Value && releaseTime >= pressTime
                ? Time.unscaledTime - releaseTime
                : -1f;
        }

        /// <summary>
        /// Sets the <see cref="Button"/> to a pressed (<see cref="true"/>) state, if it wasn't already pressed.
        /// </summary>
        public void Press()
        {
            if (Value == false)
            {
                pressAvailable = Value = true;
                releaseAvailable = false;
                pressFrame = Time.frameCount;
                pressTime = Time.unscaledTime;
                OnPressed?.Invoke(this);
                OnValueChanged?.Invoke(this);
            }
            if (!set)
                set = true;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> press is available.
        /// </summary>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> press is available.
        /// </returns>
        public bool Pressed(bool autoAccept = false)
        {
            if (set && Value && pressAvailable)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether the <see cref="Button"/> has remained pressed for a certain <paramref name="duration"/>
        /// in frames.
        /// </summary>
        /// <param name="duration">
        /// The duration (in frames) that the <see cref="Button"/> should have been pressed for.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Button"/> is pressed, and has been for the <paramref name="duration"/>
        /// or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool PressedForFrames(int duration, bool autoAccept = false)
        {
            if (duration < 0)
                throw
                    new System.ArgumentOutOfRangeException("durationFrames", ValueMustNotBeNegative("durationFrames"));
            return
                Pressed(autoAccept) && duration >= GetPressDurationFrames();
        }

        /// <summary>
        /// Checks whether the <see cref="Button"/> has remained pressed for a certain <paramref name="duration"/>
        /// in seconds.
        /// </summary>
        /// <param name="duration">
        /// The duration (in seconds) that the <see cref="Button"/> should have been pressed for.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Button"/> is pressed, and has been for the <paramref name="duration"/>
        /// or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool PressedForSeconds(float duration, bool autoAccept = false)
        {
            if (duration < 0)
                throw
                    new System.ArgumentOutOfRangeException("durationSeconds",
                    ValueMustNotBeNegative("durationSeconds"));
            return
                Pressed(autoAccept) && duration >= GetPressDurationSeconds();
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> press is available within the specified
        /// <paramref name="frameBuffer"/>. Even if the <see cref="Button"/> has since been released, a press will still
        /// be recognised if it falls within the frame buffer.
        /// </summary>
        /// <param name="frameBuffer">
        /// The amount of frames ago the unaccepted <see cref="Button"/> press may have occured to count as valid.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> press is available within the specified
        /// <paramref name="frameBuffer"/>.
        /// </returns>
        public bool PressedInFrameBuffer(int frameBuffer, bool autoAccept = false)
        {
            if (frameBuffer < 0)
                throw
                    new System.ArgumentOutOfRangeException("frameBuffer", ValueMustNotBeNegative("frameBuffer"));
            if (set && Value && pressAvailable && pressFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> press is available within the specified
        /// <paramref name="timeBuffer"/>. Even if the <see cref="Button"/> has since been released, a press will still
        /// be recognised if it falls within the time buffer.
        /// </summary>
        /// <param name="timeBuffer">
        /// The amount of seconds ago the unaccepted <see cref="Button"/> press may have occured to count as valid.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> press is available within the specified
        /// <paramref name="timeBuffer"/>.
        /// </returns>
        public bool PressedInTimeBuffer(float timeBuffer, bool autoAccept = false)
        {
            if (timeBuffer < 0)
                throw
                    new System.ArgumentOutOfRangeException("timeBuffer", ValueMustNotBeNegative("timeBuffer"));
            if (set && Value && pressAvailable && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> press is available on the current frame.
        /// </summary>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is pressed,
        /// "Pressed"-methods will not return <see cref="true"/> for that press again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> press is available on the current frame.
        /// </returns>
        public bool PressedOnCurrentFrame(bool autoAccept = false)
        {
            if (set && Value && pressAvailable && pressFrame == Time.frameCount)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Ties the <see cref="Button"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="InputAction"/> to attach to the <see cref="Button"/>.
        /// </param>
        public void RegisterInputAction(InputAction action)
        {
            if (action == null)
                throw
                    new System.ArgumentNullException("action");
            if (this.action != null)
            {
                this.action.performed -= context => Press();
                this.action.canceled -= context => Release();
            }
            SetInputAction(action);
        }

        /// <summary>
        /// Sets the <see cref="Button"/> to a released (<see cref="false"/>) state, if it wasn't already released.
        /// </summary>
        public void Release()
        {
            if (Value == true)
            {
                pressAvailable = Value = false;
                releaseAvailable = true;
                releaseFrame = Time.frameCount;
                releaseTime = Time.unscaledTime;
                OnReleased?.Invoke(this);
                OnValueChanged?.Invoke(this);
            }
            if (!set)
                set = true;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> release is available.
        /// </summary>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is released,
        /// "Released"-methods will not return <see cref="true"/> for that release again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> release is available.
        /// </returns>
        public bool Released(bool autoAccept = false)
        {
            if (set && !Value && releaseAvailable)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether the <see cref="Button"/> has remained released for a certain <paramref name="duration"/>
        /// in frames.
        /// </summary>
        /// <param name="duration">
        /// The duration (in frames) that the <see cref="Button"/> should have been released for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Button"/> is released, and has been for the <paramref name="duration"/>
        /// or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool ReleasedForFrames(int duration, bool autoAccept = false)
        {
            if (duration < 0)
                throw
                    new System.ArgumentOutOfRangeException("durationFrames", ValueMustNotBeNegative("durationFrames"));
            return
                Released(autoAccept) && duration >= GetReleasedDurationFrames();
        }

        /// <summary>
        /// Checks whether the <see cref="Button"/> has remained released for a certain <paramref name="duration"/>
        /// in seconds.
        /// </summary>
        /// <param name="duration">
        /// The duration (in seconds) that the <see cref="Button"/> should have been released for.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if the <see cref="Button"/> is released, and has been for the <paramref name="duration"/>
        /// or longer.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="duration"/> is negative.
        /// </exception>
        public bool ReleasedForSeconds(float duration, bool autoAccept = false)
        {
            if (duration < 0)
                throw
                    new System.ArgumentOutOfRangeException("durationSeconds",
                    ValueMustNotBeNegative("durationSeconds"));
            return
                Released(autoAccept) && duration >= GetReleasedDurationSeconds();
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> release is available within the specified
        /// <paramref name="frameBuffer"/>. Even if the <see cref="Button"/> has since been pressed, a release will
        /// still be recognised if it falls within the frame buffer.
        /// </summary>
        /// <param name="frameBuffer">
        /// The amount of frames ago the unaccepted <see cref="Button"/> release may have occured to count as valid.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is released,
        /// "Released"-methods not return <see cref="true"/> for that release again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> release is available within the specified
        /// <paramref name="frameBuffer"/>.
        /// </returns>
        public bool ReleasedInFrameBuffer(int frameBuffer, bool autoAccept = false)
        {

            if (frameBuffer < 0)
                throw
                    new System.ArgumentOutOfRangeException("frameBuffer", ValueMustNotBeNegative("frameBuffer"));
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame
                && releaseFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> release is available within the specified
        /// <paramref name="timeBuffer"/>. Even if the <see cref="Button"/> has since been pressed, a release will still
        /// be recognised if it falls within the time buffer.
        /// </summary>
        /// <param name="timeBuffer">
        /// The amount of seconds ago the unaccepted <see cref="Button"/> release may have occured to count as valid.
        /// </param>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is released,
        /// "Released"-methods will not return <see cref="true"/> for that release again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> release is available within the specified time buffer.
        /// </returns>
        public bool ReleasedInTimeBuffer(float timeBuffer, bool autoAccept = false)
        {
            if (timeBuffer < 0)
                throw
                    new System.ArgumentOutOfRangeException("timeBuffer", ValueMustNotBeNegative("timeBuffer"));
            if (set && !Value && releaseAvailable && releaseTime >= pressTime
                && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Checks whether an unaccepted <see cref="Button"/> release is available on the current frame.
        /// </summary>
        /// <param name="autoAccept">
        /// If set to <see cref="true"/>, (and if the <see cref="Button"/> is released,
        /// "Released"-methods will not return <see cref="true"/> for that release again.
        /// </param>
        /// <returns>
        /// <see cref="true"/> if an unaccepted <see cref="Button"/> press is available on the current frame.
        /// </returns>
        public bool ReleasedOnCurrentFrame(bool autoAccept = false)
        {
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame && releaseFrame == Time.frameCount)
            {
                if (autoAccept)
                    AcceptValue();
                return
                    true;
            }
            else
                return
                    false;
        }

        /// <summary>
        /// Toggles the <see cref="Button"/> to the opposite of the current value>.
        /// </summary>
        public void Toggle()
        {
            if (Value)
                Release();
            else
                Press();
        }

        private void SetInputAction(InputAction action)
        {
            if (this.action != null)
            {
                this.action.performed -= context => Press();
                this.action.canceled -= context => Release();
            }
            action.performed += context => Press();
            action.canceled += context => Release();
            this.action = action;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        #region Error Messages

        private static string ValueMustNotBeNegative(string valueName) => string.Format(
            "{0} value must not be negative.",
            valueName);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
