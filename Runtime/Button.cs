using UnityEngine;
using UnityEngine.InputSystem;

namespace StephanHooft.InputProcessing
{
    /// <summary>
    /// Class to process/store binary inputs such as button presses/releases.
    /// </summary>
    public class Button
    {
        /// <summary>
        /// The current value of the <see cref="Button"/>. True means pressed, whereas false means released.
        /// </summary>
        public bool Value { get; private set; } = false;

        /// <summary>
        /// How long the <see cref="Button"/> has been pressed, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of frames the <see cref="Button"/> has been pressed, and -1 if it is currently released.
        /// </returns>
        public int PressDurationFrames
        {
            get
            {
                if (set && Value && pressFrame >= releaseFrame)
                    return Time.frameCount - pressFrame;
                else
                    return -1;
            }
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been pressed, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of seconds the <see cref="Button"/> has been pressed, and -1f if it is currently released.
        /// </returns>
        public float PressDurationTime
        {
            get
            {
                if (set && Value && pressTime >= releaseTime)
                    return Time.unscaledTime - pressTime;
                else
                    return -1f;
            }
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been released, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of frames the <see cref="Button"/> has been released, and -1 if it is currently pressed.
        /// </returns>
        public int ReleasedDurationFrames
        {
            get
            {
                if (set && !Value && releaseFrame >= pressFrame)
                    return Time.frameCount - releaseFrame;
                else
                    return -1;
            }
        }

        /// <summary>
        /// How long the <see cref="Button"/> has been released, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of seconds the <see cref="Button"/> has been released, and -1f if it is currently pressed.
        /// </returns>
        public float ReleasedDurationTime
        {
            get
            {
                if (set && !Value && releaseTime >= pressTime)
                    return Time.unscaledTime - releaseTime;
                else
                    return -1f;
            }
        }

        /// <summary>
        /// Invoked when <see cref="Value"/> is set through <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/>.
        /// </summary>
        public event System.Action<bool> OnValueChanged;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool pressAvailable = false;
        private bool releaseAvailable = false;
        private int pressFrame = 0;
        private int releaseFrame = 0;
        private float pressTime = 0f;
        private float releaseTime = 0f;
        private bool set = false;
        private InputAction action;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new <see cref="Button"/>.
        /// </summary>
        public Button()
        {}

        /// <summary>
        /// Create a new <see cref="Button"/> and tie it to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="Button"/>.</param>
        public Button(InputAction action)
        {
            SetInputAction(action);
        }

        ~Button()
        {
            if(action != null)
            {
                action.performed -= context => Press();
                action.canceled -= context => Release();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set <see cref="Value"/> to a pressed (true) state, if it wasn't already pressed.
        /// <para>The <see cref="Button"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever its <see cref="Value"/> changes.</para>
        /// </summary>
        public void Press()
        {
            if (Value == false)
            {
                pressAvailable = Value = true;
                releaseAvailable = false;
                pressFrame = Time.frameCount;
                pressTime = Time.unscaledTime;
                OnValueChanged?.Invoke(true);
            }
            if (!set) 
                set = true;
        }

        /// <summary>
        /// Set <see cref="Value"/> to a released (false) state, if it wasn't already released.
        /// <para>The <see cref="Button"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever its <see cref="Value"/> changes.</para>
        /// </summary>
        public void Release()
        {
            if (Value == true)
            {
                pressAvailable = Value = false;
                releaseAvailable = true;
                releaseFrame = Time.frameCount;
                releaseTime = Time.unscaledTime;
                OnValueChanged?.Invoke(false);
            }
            if (!set) 
                set = true;
        }

        /// <summary>
        /// Toggle <see cref="Value"/> to the opposite of the current <see cref="Value"/>.
        /// <para>The <see cref="Button"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever its <see cref="Value"/> changes.</para>
        /// </summary>
        public void Toggle()
        {
            if (Value) 
                Release();
            else 
                Press();
            if (!set)
                set = true;
        }

        /// <summary>
        /// Accept the current <see cref="Value"/>. This will prevent the various "Pressed" and "Released" methods from returning true until the next time <see cref="Value"/> changes.
        /// </summary>
        public void AcceptValue()
        {
            pressAvailable = false;
            releaseAvailable = false;
        }

        /// <summary>
        /// Tie the <see cref="Button"/> to an <see cref="InputAction"/>.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to attach to the <see cref="Button"/>.</param>
        public void RegisterInputAction(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (this.action != null)
            {
                this.action.performed -= context => Press();
                this.action.canceled -= context => Release();
            }
            SetInputAction(action);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns true if an unaccepted <see cref="Button"/> press is available.
        /// <para><see cref="Button"/> presses must be "accepted" to prevent this method from returning duplicate presses. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is pressed, "Pressed"-methods will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool Pressed(bool autoAccept = false)
        {
            if (set && Value && pressAvailable)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// Returns true if an unaccepted <see cref="<see cref="Button"/>"/> press is available on the current frame.
        /// <para><see cref="Button"/> presses must be "accepted" to prevent this method from returning duplicate presses. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is pressed, "Pressed"-methods will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available on the current frame.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool PressedOnCurrentFrame(bool autoAccept = false)
        {
            if (set && Value && pressAvailable && pressFrame == Time.frameCount)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// <para>Returns true if an unaccepted <see cref="Button"/> press is available within the specified frame buffer.
        /// Even if the <see cref="Button"/> has since been released, a press will still be recognised if it falls within the frame buffer.</para>
        /// <para><see cref="Button"/> presses must be "accepted" to prevent this method from returning duplicate presses. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="frameBuffer">The amount of frames ago the unaccepted <see cref="Button"/> press may have occured to count as valid.</param>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is pressed, "Pressed"-methods will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available within the specified frame buffer.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool PressedInFrameBuffer(int frameBuffer, bool autoAccept = false)
        {
            if (frameBuffer < 0) 
                throw new System.ArgumentOutOfRangeException("frameBuffer", "frameBuffer value cannot be negative.");
            if (set && Value && pressAvailable && pressFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// <para>Returns true if an unaccepted <see cref="Button"/> press is available within the specified time buffer.
        /// Even if the <see cref="Button"/> has since been released, a press will still be recognised if it falls within the time buffer.</para>
        /// <para><see cref="Button"/> presses must be "accepted" to prevent this method from returning duplicate presses. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="timeBuffer">The amount of seconds ago the unaccepted <see cref="Button"/> press may have occured to count as valid.</param>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is pressed, "Pressed"-methods will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available within the specified time buffer.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool PressedInTimeBuffer(float timeBuffer, bool autoAccept = false)
        {
            if (timeBuffer < 0) 
                throw new System.ArgumentOutOfRangeException("timeBuffer", "timeBuffer value cannot be negative.");
            if (set && Value && pressAvailable && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// Returns true if an unaccepted <see cref="Button"/> release is available.
        /// <para><see cref="Button"/> releases must be "accepted" to prevent this method from returning duplicate releases. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is released, "Released"-methods will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release is available.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool Released(bool autoAccept = false)
        {
            if (set && !Value && releaseAvailable)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted <see cref="Button"/> release is available on the current frame.
        /// <para><see cref="Button"/> releases must be "accepted" to prevent this method from returning duplicate releases. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is released, "Released"-methods will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available on the current frame.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool ReleasedOnCurrentFrame(bool autoAccept = false)
        {
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame && releaseFrame == Time.frameCount)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted <see cref="Button"/> release is available within the specified frame buffer.
        /// Even if the <see cref="Button"/> has since been pressed, a release will still be recognised if it falls within the frame buffer.
        /// <para><see cref="Button"/> releases must be "accepted" to prevent this method from returning duplicate releases. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="frameBuffer">The amount of frames ago the unaccepted <see cref="Button"/> release may have occured to count as valid.</param>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is released, "Released"-methods not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release is available within the specified frame buffer.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool ReleasedInFrameBuffer(int frameBuffer, bool autoAccept = false)
        {

            if (frameBuffer < 0) throw new System.ArgumentOutOfRangeException("frameBuffer", "frameBuffer value cannot be negative.");
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame && releaseFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted <see cref="Button"/> release is available within the specified time buffer.
        /// Even if the <see cref="Button"/> has since been pressed, a release will still be recognised if it falls within the time buffer.
        /// <para><see cref="Button"/> releases must be "accepted" to prevent this method from returning duplicate releases. This can be done by setting <paramref name="autoAccept"/>
        /// to true, or by calling <see cref="AcceptValue"/> manually.</para>
        /// </summary>
        /// <param name="timeBuffer">The amount of seconds ago the unaccepted <see cref="Button"/> release may have occured to count as valid.</param>
        /// <param name="autoAccept">If set to true, (and if the <see cref="Button"/> is released, "Released"-methods will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release is available within the specified time buffer.
        /// <para>This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.</para>
        /// </returns>
        public bool ReleasedInTimeBuffer(float timeBuffer, bool autoAccept = false)
        {
            if (timeBuffer < 0) throw new System.ArgumentOutOfRangeException("timeBuffer", "timeBuffer value cannot be negative.");
            if (set && !Value && releaseAvailable && releaseTime >= pressTime && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAccept) 
                    AcceptValue();
                return true;
            }
            else 
                return false;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void SetInputAction(InputAction action)
        {
            if (action == null)
                throw new System.ArgumentNullException("action");
            if (this.action != null)
            {
                this.action.performed -= context => Press();
                this.action.canceled -= context => Release();
            }
            action.performed += context => Press();
            action.canceled += context => Release();
            this.action = action;
        }
    }
}
