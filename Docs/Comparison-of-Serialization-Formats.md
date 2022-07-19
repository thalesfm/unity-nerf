# Comparison of Serialization Formats

**JSON** is arguably the most popular and widely supported serialization format of the bunch. It has been designed to resemble JavaScript syntax, making it human-readable but very inefficient both in terms of space and encoding/decoding performance.

**BSON** is a binary interchange format based on JSON designed to be more efficient both in terms of storage and scan speed. In practice however, our experiments show that BSON files have an even larger storage footprint than JSON when used for storing PlenOctree data.

The C# standard library features its own binary serialization format called **BinaryFormatter**. Although it is widely supported within the .NET ecosystem, it is not cross-platform and thus difficult to work within other contexts.

Protocol Buffers, or **Protobuf** for short, is a binary serialization format developed by Google. It is designed to be efficient, cross-platform, and easy to use. Unlike JSON, Protobuf is designed for structured data -- that means encoding/decoding rely on pre-defined data schemas supplied by the programmer in the form of `.proto` files. These files are written in a custom schema specification language.

**Flatbuffers** is yet another binary format for structured data, also developed by Google. It is meant to address some of the shortcomings of Protobuf in terms of performance, particularly when working with larger collections of data or when partial decoding of records is required. Our experiments show that it is more efficient than Protobuf in terms of storage size, but encoding speed suffers considerably (however this could be an issue specific to the Python library used; further experiments would be good).

**Cap'n Proto** is a serialization protocol created by the former maintainer of the Protocol Buffers library. It was designed to improve on some of Protocol Buffers' shortcomings by including, for example, deeper support for generic and dynamic types. Unofficial community implementations of the protocol are available for a number of languages; however these implementations tend to be less mature than those of Protocol Buffers and Flatbuffers due to the fact that Cap'n Proto has a considerably smaller community.

## Features

Format | Binary | Cross-platform | Language Support (Python/C#) | Packed Lists | Generic Types
------ | ------ | -------------- | ---------------------------- | ------------ | -------------
JSON            | No  | Yes | Yes/Yes | No  | Yes
BSON            | Yes | Yes | Yes/Yes | No  | Yes
BinaryFormatter | Yes | No  | No/Yes  | Yes | Yes
Protobuf        | Yes | Yes | Yes/Yes | Yes | Yes‡
FlatBuffers     | Yes | Yes | Yes/Yes | Yes | No
Cap'n Proto     | Yes | Yes | No/No\* | Yes | Yes
HDF5            | Yes | Yes | No/No†  | Yes | No

\* Unofficial community support available for Python ([pycapnproto](https://capnproto.github.io/pycapnp/)) and C# ([capnproto-dotnetcore](https://github.com/c80k/capnproto-dotnetcore)).

† Unofficial community support available for Python ([h5py](https://www.h5py.org/)) and C# ([HDF5-CSharp](https://github.com/LiorBanai/HDF5-CSharp)).

‡ Arbitrary types can be represented using [`Any`](https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#any), which "contains an arbitrary serialized message along with a URL that describes the type." Unfortunately, this feature is not well supported among implementations. Additionally, `Any` requires list entries to be individually tagged, defeating the purpose of packed lists. A workaround for this issue is to represent lists of generic values using the `bytes` type and store an URL describing the type as a separate field of the message. This is what we do in practice, although it is not ideal.

## Benchmarks

Format          | Encoding Time (s) | Decoding Time (s) | File Size (MB)
--------------- | ----------------: | ----------------: | -------------:
JSON            |              35.5 |               --- |          244.8
BSON            |              18.0 |               --- |          421.0
BinaryFormatter |               --- |               --- |            ---
Protobuf        |               4.4 |               --- |          120.0
FlatBuffers     |              60.0 |               --- |           96.0
Cap'n Proto     |               6.8 |               --- |           96.0
HDF5            |               --- |               --- |            --- 

Results measured in Google Colab using the LEGO sample scene ([Google Drive](https://drive.google.com/drive/folders/1J0lRiDn_wOiLVpCraf6jM7vvCwDr9Dmx?usp=sharing)) released by Alex Yu et al. The scene was converted from its original N3Tree representation into a SparseVoxelOctree and resampled at a resolution of 128<sup>3</sup>.
