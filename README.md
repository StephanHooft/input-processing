# input-processing
This Unity package contains a series of classes that help with the storage and processing of (user) inputs. 

While Unity's own Input System is very useful for abstracting input on the hardware level, there are some limitations on the processing front. InputActions fire ``performed`` and ``canceled`` events when input is detected, but the context of these events is meant to be discarded as soon as they are processed. That leaves a gap when evaluating inputs over a longer range of time. The classes in this package serve as an intermediary to that effect.
## Button
The ``Button`` class stores pressed/released (``bool``) input values, and offers some specialised helper methods to interpret button presses/releases over time.
### Setting Button values
A ``Button``'s value can be updated in two primary ways:
1. Manually, through the ``Button.Press()``, ``Button.Release()``, or ``Button.Toggle()`` methods.
2. By attaching an ``InputAction`` to the Button with ``Button.RegisterInputAction()``, or by specifying the ``InputAction`` upon instantiation. This causes the ``Button`` to subscribe to the ``performed`` and ``canceled`` events of said ``InputAction``, and removes the need to use manual methods listed above.
### Reading Button values
A ``Button`` whose value is reliably updated can be read from. A ``Button`` offers various ways of doing so:
1. The ``bool`` property ``Button.Value`` returns the most recently set value of the Button.
2. How long a ``Button`` has been pressed or released in frames (``int``) or seconds (``float``) is returned by properties such as ``Button.PressDurationFrames``, ``Button.PressDurationTime``, ``Button.ReleaseDurationFrames``, and ``Button.ReleaseDurationTime``.
3. Methods such as ``Button.Pressed()``, ``Button.PressedOnCurrentFrame()``, ``Button.Released()``, and ``Button.ReleasedOnCurrentFrame()`` help with detecting individual button presses/releases. ``Button.Accept()`` or optional parameters in the prior methods can be used to acknowledge such button presses/releases, so the ``Button`` doesn't return them more than once.
4. The ``Button`` remembers when it was last pressed/released. This can be used to acknowledge presses/releases that happened before the current frame, thus enabling "buffered" button presses. The methods ``Button.PressedInFrameBuffer()``, ``Button.PressedInTimeBuffer()``, ``Button.ReleasedInFrameBuffer()``, and ``Button.ReleasedInTimeBuffer()`` can be used for this.
5. The ``Button`` exposes events (``Button.OnValueChanged``, ``Button.OnPressed``, and ``Button.OnReleased``), which can be subscribed to.
## Axis
The ``Axis`` class stores axial (``float``) values, as used by triggers and sticks. (Using TwinAxes is recommended for sticks.) Like the ``Button`` class, it offers helper methods to interpret 
### Setting Axis values
An ``Axis``' value can be updated in two primary ways:
1. Manually, through the ``Axis.SetValue()`` method.
2. By attaching an ``InputAction`` to the ``Axis`` with ``Axis.RegisterInputAction()``, or by specifying the ``InputAction`` upon instantiation. This causes the ``Axis`` to subscribe to the ``performed`` and ``canceled`` events of said ``InputAction``, and removes the need to use manual methods listed above. The ``InputAction`` **MUST** have "Axis" as its expected Control Type.
### Reading Axis values
An ``Axis`` whose value is reliably updated can be read from. An ``Axis`` offers various ways of doing so:
1. The ``float`` property ``Axis.Value`` returns the most recently set value of the Axis.
2. The ``bool`` properties ``Axis.Positive``, ``Axis.Neutral``, and ``Axis.Negative`` can be called on to evaluate the current ``Axis``' value for conditional statements.
3. How long an ``Axis`` has been positive (> 0f), neutral (== 0f), or negative (< 0f) can be calculated in frames (``int``) or seconds (``float``) using methods such as ``Axis.PositiveDurationFrames()``, ``Axis.PositiveDurationTime()``, ``Axis.NeutralDurationFrames()``, ``Axis.NeutralDurationTime()``, ``Axis.GetNegativeDurationFrames()``, and ``Axis.GetNegativeDurationTime()``.
4. The ``Axis`` exposes events (``Axis.OnValueChanged``, ``Axis.OnPositive``, ``Axis.OnNeutral``, and ``Axis.OnNegative``), which can be subscribed to.
## TwinAxes
The ``TwinAxes`` class stores a pair (``Vector2``) of ``Axis`` instances, making it highly recommended for joy-/control-sticks. It has some dedicated methods to easily manage and read both ``Axis`` at once.
### Setting TwinAxes values
A ``TwinAxes``' value can be updated in three primary ways:
1. Manually, through the ``TwinAxes.SetValue()`` method.
2. Manually, by calling ``TwinAxes.AxisX.SetValue()`` or ``TwinAxes.AxisY.SetValue()``.
3. By attaching an ``InputAction`` to the TwinAxes with ``TwinAxes.RegisterInputAction()``, or by specifying the ``InputAction`` upon instantiation. This causes the ``TwinAxes`` to subscribe to the ``performed`` and ``canceled`` events of said ``InputAction``, and removes the need to use manual methods listed above. The ``InputAction`` **MUST** have "Vector2" as its expected Control Type.
### Reading TwinAxes values
A ``TwinAxes`` whose value is reliably updated can be read from. A ``TwinAxes`` offers various ways of doing so:
1. The ``Vector2`` property ``TwinAxes.Value`` returns the most recently set value of the ``TwinAxes``.
2. The ``float`` property ``TwinAxes.Angle`` returns the angle of the ``TwinAxes``.
3. The ``TwinAxes.AxisX`` and ``TwinAxes.AxisY`` properties allow for each ``Axis`` to be directly accessed as described above.
4. The ``bool`` property ``TwinAxes.Neutral`` can be called on to evaluate the current ``TwinAxes`` value for conditional statements.
5. The class exposes an event (``TwinAxes.OnValueChanged``), which can be subscribed to.
