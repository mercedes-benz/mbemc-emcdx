// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text;

namespace MBN50284;

class RsWaveform
{
    #region Private Fields

    static readonly Encoding Ascii = Encoding.ASCII;

    #endregion Private Fields

    #region Private Methods

    static void WriteAscii(BinaryWriter writer, string ascii) => writer.Write(Ascii.GetBytes(ascii));

    #endregion Private Methods

    #region Public Methods

    /// <summary>Writes a file containing "Rhode &amp; Schwarz waveform with tag file format".</summary>
    /// <param name="fileName">Filename to write to (extension will be replaced with .wv)</param>
    /// <returns>Returns the IQData read.</returns>
    /// <exception cref="NotImplementedException">This function is only implemented for little endian machines!</exception>
    /// <exception cref="InvalidDataException">Invalid data found at RS waveform.</exception>
    public static IQData Read(string fileName)
    {
        if (!BitConverter.IsLittleEndian) throw new NotImplementedException("This function is only implemented for little endian machines!");
        Console.WriteLine($"... Reading RS waveform file {fileName}");
        var bytes = File.ReadAllBytes(fileName);
        var startBlock = -1;
        var i = 0;
        long fileSampleCount = 0;
        IQData data = new()
        {
            Name = Path.GetFileNameWithoutExtension(fileName)
        };

        void MoveToEndOfTag() { while (bytes[++i] != '}') ; startBlock = -1; }
        string ReadTag()
        {
            var start = i;
            MoveToEndOfTag();
            return Ascii.GetString(bytes[start..i]);
        }
        void ParseTag(string tag)
        {
            switch (tag)
            {
                default: MoveToEndOfTag(); break;
                case "CLOCK": data = data with { SampleRate = long.Parse(ReadTag()) }; break;
                case "COMMENT": data = data with { Description = ReadTag() }; break;
                case "SAMPLES": fileSampleCount = long.Parse(ReadTag(), CultureInfo.InvariantCulture); break;
                case "LEVEL OFFS":
                {
                    var rsLevelOffs = ReadTag().Split(',');
                    data = data with
                    {
                        RootMeanSquareOffset = double.Parse(rsLevelOffs[0], CultureInfo.InvariantCulture),
                        PeakOffset = double.Parse(rsLevelOffs[1], CultureInfo.InvariantCulture),
                    };
                    break;
                }
            }
        }
        void ReadBinary(string tag)
        {
            var size = long.Parse(tag[(tag.IndexOf('-') + 1)..]);
            var sampleCount = (size - 1) / 4;
            if (fileSampleCount != sampleCount) throw new InvalidDataException($"SampleCount at samples tag {fileSampleCount} does not match size of waveform tag {size}!");
            if ((sampleCount * 4) + 1 != size) throw new InvalidDataException("Invalid data size at rs waveform!");
            if (bytes[i++] != '#') throw new InvalidDataException("Invalid start tag at rs waveform!");
            var real = new double[sampleCount];
            var imag = new double[sampleCount];
            for (var n = 0; n < sampleCount; n++)
            {
                real[n] = BitConverter.ToInt16(bytes, i) / (double)short.MaxValue;
                i += 2;
                imag[n] = BitConverter.ToInt16(bytes, i) / (double)short.MaxValue;
                i += 2;
            }
            data = data with { Real = real, Imaginary = imag };
        }
        void CheckTag()
        {
            if (bytes[i++] == ':')
            {
                var tag = Ascii.GetString(bytes[(startBlock + 1)..(i - 1)]);
                if (tag.StartsWith("WAVEFORM-"))
                {
                    ReadBinary(tag);
                }
                else
                {
                    ParseTag(tag);
                }
            }
        }

        for (; i < bytes.Length;)
        {
            if (startBlock >= 0)
            {
                CheckTag();
            }
            else if (bytes[i] == '{')
            {
                startBlock = i;
            }
            else
            {
                i++;
            }
        }
        return data;
    }

    /// <summary>Writes a file containing "Rhode &amp; Schwarz waveform with tag file format".</summary>
    /// <param name="fileName">File to write to.</param>
    /// <param name="data">iq data to write</param>
    /// <param name="description">Description to be added to the comment field</param>
    /// <returns>Returns the filename of the file written.</returns>
    /// <exception cref="NotImplementedException">This function is only implemented for little endian machines!</exception>
    public static string Write(string fileName, IQData data, string? description)
    {
        if (!BitConverter.IsLittleEndian) throw new NotImplementedException("This function is only implemented for little endian machines!");
        fileName = Path.ChangeExtension(fileName, ".wv");
        Console.WriteLine($"... Writing RS waveform file {fileName}");
        using var fileStream = File.Create(fileName);
        var writer = new BinaryWriter(fileStream, Encoding.ASCII);
        var rsDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var rsLength = (data.SampleCount * 4) + 1;
        var rsLevelOffs = data.RootMeanSquareOffset.ToString("R", CultureInfo.InvariantCulture) + "," + data.PeakOffset.ToString("R", CultureInfo.InvariantCulture);

        WriteAscii(writer,
            $"{{TYPE: SMU-WV,0}}\n" +
            $"{{CLOCK: {(long)data.SampleRate}}}\n" +
            $"{{COMMENT: {description ?? data.Description}}}\n" +
            $"{{COPYRIGHT: Mercedes-Benz AG}}\n" +
            $"{{DATE: {rsDate}}}\n" +
            $"{{SAMPLES: {data.SampleCount}}}\n" +
            $"{{LEVEL OFFS: {rsLevelOffs}}}\n" +
            $"{{WAVEFORM-{rsLength}:#");

        checked
        {
            foreach (var value in data.Complex)
            {
                var real = (short)Math.Floor((value.Real * short.MaxValue) + 0.5);
                var imag = (short)Math.Floor((value.Imaginary * short.MaxValue) + 0.5);
                writer.Write(real);
                writer.Write(imag);
            }
        }

        WriteAscii(writer, "}");
        writer.Close();
        return fileName;
    }

    #endregion Public Methods
}
