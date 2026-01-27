# Mbemc.DataExchange

**Mbemc.DataExchange** is a C# library for easy storage and loading of measurement data using attribute-based class declarations in the **EMC Data Exchange Format**, first described in MBN 50284-2 (2023).

## Installation

Install the package via NuGet:

```shell
dotnet add package mbemc-emcdx
```

## Usage

### Example Class

The following class can be automatically saved and loaded:

```csharp
class SampleData : IDxName, IDxDescription
{
    public string Name { get; set; }
    public string Description { get; set; }
    [DxVariable]
    public double[] Values { get; set; }
}
```

### Saving Data

```csharp
var sample = new SampleData()
{
    Name = "Sample Data",
    Values = new double[] { 1.23, 2.34, 3.45, 4.56, 5.67, 10.0 }
};

var writer = new DxWriter(name: "sample");
writer.Write(sample);
writer.Save();
```

### Loading Data

```csharp
var reader = new DxReader.ReadFile("sample.emcdx");
var samples = reader.Read<SampleData>().ToList();
```

### Memory Usage and File Size Limitations
This library loads all data fully into memory.
Please ensure that sufficient RAM is available, especially when working with large files.

**Maximum Supported Fule Sizes:**
- Emcdx files (.emcdx): up to ~128 MB
- Binary files (.bin and .txt): up to 2 GB

Exceeding these limits will lead to exceptions. Make sure your application is running with adequate memory resources.

**Note on Multi-User Environments:**
In multi-user scenarios such as server applications, simultaneous processing should be limited by the consumer of the library to maintain a deterministic memory footprint. This helps avoid unpredictable resource allocation and ensures consistent performance.


## EMC Data Exchange Format

The **EMC Data Exchange Format** is based on JSON and is designed for exchanging large measurement and simulation data. It defines structured JSON objects for measurement variables (`Variables`) and mappings (`Mappings`) that represent complex measurement data as N-dimensional arrays.

### **Main Components of an EMC File**
A file in the **EMC Data Exchange Format** consists of several JSON objects that contain measurement data and its metadata:

1. **Data** – The main object that describes a measurement or simulation. It includes:
   - A list of `Variables` that define measurement quantities such as time or frequency.
   - A list of `Mappings` that associate measurement values with corresponding variables.

2. **Variables (Measurement Quantities)** – Defines the dimensions and parameters of the data. They can be:
   - Values in the form of lists (`ValueList`)
   - Computed values with start, step size, and count (`Generator`)
   - Binary stored values (`BinaryValues`)

3. **Mappings (Measurement Values)** – Describes the data of measurements or simulations. Each mapping contains:
   - A list of variables that define the dimensions of the data.
   - The actual measurement values, either as a JSON array or as `BinaryValues`.

### **Support for Binary Data**
Since some measurements produce large data volumes (e.g., oscilloscope or IQ data), the format supports **binary storage** via `BinaryValues`. These contain:
- **Dimensions** of the data (e.g., number of points on each axis)
- **Data format** (`double`, `int16`, etc.)
- **Scaling** and **offset** to correctly represent measurement values
- **Storage block** that stores the binary data as base64-encoded content or references external files

### **Integration with Simulations**
In addition to pure measurement data, simulation results can also be stored in this format. Variables for physical parameters such as temperature or spatial direction can be saved and used as the basis for simulations.

### **Why JSON?**
The EMC format relies on JSON because it is:
- **Easily readable**
- **Widely used** and easy to parse
- **Flexibly extendable** to support future measurement methods

With this structure, the **EMC Data Exchange Format** provides a powerful solution for exchanging complex measurement and simulation data.

### Example

```json
{
  "Type": "EMC Data Exchange Format",
  "Version": "2.0",
  "Name": "Sample File",
  "Data": {
    "Name": "Sample Measurement",
    "Description": "Measurement of currents in BCI tests",
    "Variables": [
      {
        "Name": "Frequency",
        "Unit": "MHz",
        "Generator": {
          "Start": 0.1,
          "Step": 0.1,
          "Count": 100
        }
      },
      {
        "Name": "Current",
        "Unit": "dB(μA)",
        "BinaryValues": {
          "Dimensions": [100],
          "Format": "Float64",
          "Endian": "LittleEndian",
          "Scale": 1.0,
          "Offset": 0.0,
          "Content": "QZmJ9OXIT0Bl59Hyv+5PQNmRF0UpOE9ASXIu4Cj+TUBd/YlbsmY9QAeJW/3mCT0DYxXQhWt49QEpHlnMvzk5A7LVlmklWj0B1FND6ySA9QYfp7QhExj0DIKrTx+oZE9QYsU9lMeiD0DYOhZzFowgQGuHTwA1SUNA7IVTnB6hNQEpKL+Z4YzZAzIdIkISpj0Bw9VvRrBhVQO+jNZnHR0FAjKo25JHCWUCoUlqPzOTdAghbF0KrVlVBcPwFxywmlUEAlmjwFeMWUBGVQz1wymZQTFTJPaUMxlFBVFFs20csYUEXWDLTLOljQQBkC55eAWFBAaEHFIHxgUUJRTpoBlsVBQ3IQo/CHYEFAKfn7UyBiQRWj78KL+4hAw=="
        }
      }
    ],
    "Mappings": [
      {
        "Name": "Measurement Value",
        "Variables": ["Frequency"],
        "BinaryValues": {
          "Dimensions": [100],
          "Format": "Float64",
          "Endian": "LittleEndian",
          "Scale": 1.0,
          "Offset": 0.0,
          "Source": "measurement-values.bin"
        }
      }
    ]
  }
}
```

### **Explanation of the JSON Structure:**
- **`Data`**: Contains the entire measurement.
- **`Variables`**: Lists the measurement quantities, such as frequency (as computed values via `Generator`) and current (as `BinaryValues` for binary data).
- **`Mappings`**: Contains the actual measurement values and assigns them to the variables.
- **`BinaryValues`**: Stores data either base64-encoded in JSON or as a separate file in binary format to handle large data sets efficiently.

## Development Environment

### Supported Frameworks
The project requires the following .NET SDKs / .NET target versions:
- `.NET Framework 4.6.2`
- `.NET Standard 2.0`
- `.NET 8.0`

### Code Quality and Analysis Tools
The project uses `.NET Analyzers` with the following settings:
- `Warning Level 5`: Strict checks to identify potential code issues early.
- `Nullable Enable`: Enabled nullability for improved type safety and error prevention.

### Additional Tools
To maintain code style and generate documentation, the following tools are required:
- **Sandcastle Help File Builder**: Used for creating technical documentation.
- **Code Maid Visual Studio Extension**: Supports code cleaning and refactoring within Visual Studio.

This repository is intended to vet PRs for
<https://github.com/mercedes-benz/emv-data-exchange>
w.r.t. Mercedes's internal FOSS contribution rules. It can also be
used for "sheltered" internal technical discussions before
contributiors face the harsh reality of the wider public's opinion.
contributiors face the harsh reality of the wider public's opinion.

## License

This package is licensed under the MIT License. See the LICENSE file for more details.

In the context of this repository, all contributions are licensed
under *BOTH*, the MIT license and the Mercedes-Benz ISL.  When
submitting upstream, the general public will *only* see the MIT
license. (Please be aware that the Mercedes-Benz ISL *MUST NOT* be
exposed outside of Mercedes-Benz.)
