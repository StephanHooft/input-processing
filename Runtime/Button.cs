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
        private readonly InputAction action;

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
            if (action == null) 
                throw new System.ArgumentNullException("action");
            action.performed += context => Press();
            action.canceled += context => Release();
            this.action = action;
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

                // Let our listeners know that the value of the Button has changed.
                OnValueChanged?.Invoke(false);
            }
            if (!set) set = true;
        }

        /// <summary>
        /// Toggle <see cref="Value"/> to the opposite of the current <see cref="Value"/>.
        /// <para>The <see cref="Button"/> logs the <see cref="Time.frameCount"/> and <see cref="Time.unscaledTime"/> whenever its <see cref="Value"/> changes.</para>
        /// </summary>
        public void Toggle()
        {
            if (Value) Release();
            else Press();
        }

        /// <summary>
        /// Accept the current <see cref="Value"/>. This will prevent the various "Pressed" and "Released" methods from returning true until <see cref="Value"/> changes.
        /// </summary>
        public void AcceptValue()
        {
            pressAvailable = false;
            releaseAvailable = false;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns true if an unaccepted(!) <see cref="Button"/> press is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate presses.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per press.</para>
        /// </summary>
        /// <param name="autoAcceptValueIfPressed">If set to true, (and if the <see cref="Button"/> is pressed, the <see cref="Button"/> will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool Pressed(bool autoAcceptValueIfPressed = false)
        {
            if (set && Value && pressAvailable)
            {
                if (autoAcceptValueIfPressed) AcceptValue();
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted(!) <see cref="Button"/> press on the current frame is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate presses.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per press.</para>
        /// </summary>
        /// <param name="autoAcceptValueIfPressed">If set to true, (and if the <see cref="Button"/> is pressed, the <see cref="Button"/> will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press on the current frame is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool PressedOnCurrentFrame(bool autoAcceptValueIfPressed = false)
        {
            if (set && Value && pressAvailable && pressFrame == Time.frameCount)
            {
                if (autoAcceptValueIfPressed) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// <para>Returns true only if an unaccepted(!) <see cref="Button"/> press within the specified frame buffer is available.</para>
        /// <para>Even if the button was released in the meantime, a press within the frame buffer will still be recognised.</para>
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate presses.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per press.</para>
        /// </summary>
        /// <param name="frameBuffer">The amount of frames ago the unaccepted <see cref="Button"/> press may have occured to count as valid.</param>
        /// <param name="autoAcceptValueIfPressed">If set to true, (and if the <see cref="Button"/> is pressed, the <see cref="Button"/> will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press within the specified frame buffer is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool PressedBuffered(int frameBuffer, bool autoAcceptValueIfPressed = false)
        {
            if (frameBuffer < 0) throw new System.ArgumentOutOfRangeException("frameBuffer", "frameBuffer value cannot be negative.");
            if (set && Value && pressAvailable && pressFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAcceptValueIfPressed) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// <para>Returns true only if an unaccepted(!) <see cref="Button"/> press within the specified time buffer is available.</para>
        /// <para>Even if the button was released in the meantime, a press within the time buffer will still be recognised.</para>
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate presses.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per press.</para>
        /// </summary>
        /// <param name="timeBuffer">The amount of seconds ago the unaccepted <see cref="Button"/> press may have occured to count as valid.</param>
        /// <param name="autoAcceptValueIfPressed">If set to true, (and if the <see cref="Button"/> is pressed, the <see cref="Button"/> will not return true for that press again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press within the specified time buffer is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool PressedBuffered(float timeBuffer, bool autoAcceptValueIfPressed = false)
        {
            if (timeBuffer < 0) throw new System.ArgumentOutOfRangeException("timeBuffer", "timeBuffer value cannot be negative.");
            if (set && Value && pressAvailable && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAcceptValueIfPressed) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// Returns the amount of frames that the <see cref="Button"/> has been pressed, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of frames the <see cref="Button"/> has been pressed, if it currently is. If <see cref="Value"/> is currently not true, this method will return -1.
        /// This method will also always return -1 if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public int PressedDurationFrames()
        {
            if (set && Value && pressFrame >= releaseFrame)
                return Time.frameCount - pressFrame;

            else return -1;
        }

        /// <summary>
        /// Returns the amount of seconds that the <see cref="Button"/> has been pressed, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of seconds the <see cref="Button"/> has been pressed, if it currently is. If <see cref="Value"/> is currently not true, this method will return -1.
        /// This method will also always return -1 if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public float PressedDurationTime()
        {
            if (set && Value && pressTime >= releaseTime)
                return Time.unscaledTime - pressTime;

            else return -1f;
        }

        /// <summary>
        /// Returns true if an unaccepted(!) <see cref="Button"/> release is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate releases.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per release.</para>
        /// </summary>
        /// <param name="autoAcceptValueIfReleased">If set to true, (and if the <see cref="Button"/> is released, the <see cref="Button"/> will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool Released(bool autoAcceptValueIfReleased = false)
        {
            if (set && !Value && releaseAvailable)
            {
                if (autoAcceptValueIfReleased) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted(!) <see cref="Button"/> release on the current frame is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate releases.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per release.</para>
        /// </summary>
        /// <param name="autoAcceptValueIfReleased">If set to true, (and if the <see cref="Button"/> is released, the <see cref="Button"/> will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> press is available on the current frame.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool ReleasedOnCurrentFrame(bool autoAcceptValueIfReleased = false)
        {
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame && releaseFrame == Time.frameCount)
            {
                if (autoAcceptValueIfReleased) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted(!) <see cref="Button"/> release within the specified frame buffer is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate releases.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per release.</para>
        /// </summary>
        /// <param name="frameBuffer">The amount of frames ago the unaccepted <see cref="Button"/> release may have occured to count as valid.</param>
        /// <param name="autoAcceptValueIfReleased">If set to true, (and if the <see cref="Button"/> is released, the <see cref="Button"/> will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release within the specified frame buffer is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool ReleasedBuffered(int frameBuffer, bool autoAcceptValueIfReleased = false)
        {

            if (frameBuffer < 0) throw new System.ArgumentOutOfRangeException("frameBuffer", "frameBuffer value cannot be negative.");
            if (set && !Value && releaseAvailable && releaseFrame >= pressFrame && releaseFrame >= Time.frameCount - frameBuffer)
            {
                if (autoAcceptValueIfReleased) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// Returns true only if an unaccepted(!) <see cref="Button"/> release within the specified time buffer is available.
        /// <para>If this method returns true, <see cref="AcceptValue"/> should be called afterwards to prevent the <see cref="Button"/> from returning duplicate releases.</para>
        /// <para>Alternatively, if <paramref name="autoAcceptValueIfPressed"/> is set to true, this method will only return true once per release.</para>
        /// </summary>
        /// <param name="timeBuffer">The amount of seconds ago the unaccepted <see cref="Button"/> release may have occured to count as valid.</param>
        /// <param name="autoAcceptValueIfReleased">If set to true, (and if the <see cref="Button"/> is released, the <see cref="Button"/> will not return true for that release again.</param>
        /// <returns>
        /// True if an unaccepted <see cref="Button"/> release within the specified time buffer is available.
        /// This method will always return false if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public bool ReleasedBuffered(float timeBuffer, bool autoAcceptValueIfReleased = false)
        {
            if (timeBuffer < 0) throw new System.ArgumentOutOfRangeException("timeBuffer", "timeBuffer value cannot be negative.");
            if (set && !Value && releaseAvailable && releaseTime >= pressTime && pressTime >= Time.unscaledTime - timeBuffer)
            {
                if (autoAcceptValueIfReleased) AcceptValue();
                return true;
            }

            else return false;
        }

        /// <summary>
        /// Returns the amount of frames that the <see cref="Button"/> has been released, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of frames the <see cref="Button"/> has been released, if it currently is. If <see cref="Value"/> is currently not false, this method will return -1.
        /// This method will always return -1 if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public int ReleasedDurationFrames()
        {
            if (set && !Value && releaseFrame >= pressFrame)
                return Time.frameCount - releaseFrame;

            else return -1;
        }

        /// <summary>
        /// Returns the amount of seconds that the <see cref="Button"/> has been released, if it currently is.
        /// </summary>
        /// <returns>
        /// The amount of seconds the <see cref="Button"/> has been released, if it currently is. If <see cref="Value"/> is currently not false, this method will return -1.
        /// This method will also always return -1 if <see cref="Press"/>, <see cref="Release"/>, or <see cref="Toggle"/> have never been called.
        /// </returns>
        public float ReleasedDurationTime()
        {
            if (set && !Value && releaseTime >= pressTime)
                return Time.unscaledTime - releaseTime;

            else return -1f;
        }
    }
}
