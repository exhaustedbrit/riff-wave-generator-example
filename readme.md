# Sound Generator Example

This example project implements a basic PCM WAVE generator, with methods to
generate a sine tone. It is intended to be somewhat educational and introduce
the concepts of IO, digital audio and also serve as a complete application open
to modification by anyone hoping to dip their toes in digital sound.

## Requirements

The project is written in C# and has no third-party dependencies and targets
the .NET Core 3.1 Framework, but should be compatible with many older versions
of .NET.

I personally recommend using Visual Studio or Visual Studio Code.

If you have `dotnet` installed, you can run `dotnet run Program.cs` in a
terminal to output the `sound.wav` example.

## Explanation

The code consists of two main components, they are called upon in the `Main`
method.

### RIFFWaveFile

This class can output a valid WAVE file given PCM data. It has an adjustable
sample rate and could be expanded on to allow multiple streams (stereo).

### AudioGenerator

This class contains static methods to generate raw PCM waveforms. It has a
method to generate a sinusoidal wave at a given frequency, and additionally a
similar method to generate a sinusoidal wave sweep between two target
frequencies.

The class could be expanded on to generate different types of sound, such as
noise or other waveform shapes.

## Contributions

The nature of this codebase is to provide a little insight into file formats and
digital sound in a tangible and fun way. Please feel free to make improvements
and corrections so long as they are in the spirit of keeping it easily
digestible and useful for someone with limited experience.