// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: svo.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace SVO.Protobuf {

  /// <summary>Holder for reflection information generated from svo.proto</summary>
  public static partial class SvoReflection {

    #region Descriptor
    /// <summary>File descriptor for svo.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SvoReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cglzdm8ucHJvdG8SDHN2by5wcm90b2J1ZiKBAQoRU3BhcnNlVm94ZWxPY3Ry",
            "ZWUSEAoIdHlwZV91cmwYASABKAkSDQoFd2lkdGgYAiABKAUSDgoGaGVpZ2h0",
            "GAMgASgFEg0KBWRlcHRoGAQgASgFEhkKDW5vZGVfY2hpbGRyZW4YBSADKAVC",
            "AhABEhEKCW5vZGVfZGF0YRgGIAEoDEIPqgIMU1ZPLlByb3RvYnVmYgZwcm90",
            "bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::SVO.Protobuf.SparseVoxelOctree), global::SVO.Protobuf.SparseVoxelOctree.Parser, new[]{ "TypeUrl", "Width", "Height", "Depth", "NodeChildren", "NodeData" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class SparseVoxelOctree : pb::IMessage<SparseVoxelOctree> {
    private static readonly pb::MessageParser<SparseVoxelOctree> _parser = new pb::MessageParser<SparseVoxelOctree>(() => new SparseVoxelOctree());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SparseVoxelOctree> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SVO.Protobuf.SvoReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SparseVoxelOctree() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SparseVoxelOctree(SparseVoxelOctree other) : this() {
      typeUrl_ = other.typeUrl_;
      width_ = other.width_;
      height_ = other.height_;
      depth_ = other.depth_;
      nodeChildren_ = other.nodeChildren_.Clone();
      nodeData_ = other.nodeData_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SparseVoxelOctree Clone() {
      return new SparseVoxelOctree(this);
    }

    /// <summary>Field number for the "type_url" field.</summary>
    public const int TypeUrlFieldNumber = 1;
    private string typeUrl_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string TypeUrl {
      get { return typeUrl_; }
      set {
        typeUrl_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "width" field.</summary>
    public const int WidthFieldNumber = 2;
    private int width_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Width {
      get { return width_; }
      set {
        width_ = value;
      }
    }

    /// <summary>Field number for the "height" field.</summary>
    public const int HeightFieldNumber = 3;
    private int height_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Height {
      get { return height_; }
      set {
        height_ = value;
      }
    }

    /// <summary>Field number for the "depth" field.</summary>
    public const int DepthFieldNumber = 4;
    private int depth_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Depth {
      get { return depth_; }
      set {
        depth_ = value;
      }
    }

    /// <summary>Field number for the "node_children" field.</summary>
    public const int NodeChildrenFieldNumber = 5;
    private static readonly pb::FieldCodec<int> _repeated_nodeChildren_codec
        = pb::FieldCodec.ForInt32(42);
    private readonly pbc::RepeatedField<int> nodeChildren_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> NodeChildren {
      get { return nodeChildren_; }
    }

    /// <summary>Field number for the "node_data" field.</summary>
    public const int NodeDataFieldNumber = 6;
    private pb::ByteString nodeData_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString NodeData {
      get { return nodeData_; }
      set {
        nodeData_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SparseVoxelOctree);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SparseVoxelOctree other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (TypeUrl != other.TypeUrl) return false;
      if (Width != other.Width) return false;
      if (Height != other.Height) return false;
      if (Depth != other.Depth) return false;
      if(!nodeChildren_.Equals(other.nodeChildren_)) return false;
      if (NodeData != other.NodeData) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (TypeUrl.Length != 0) hash ^= TypeUrl.GetHashCode();
      if (Width != 0) hash ^= Width.GetHashCode();
      if (Height != 0) hash ^= Height.GetHashCode();
      if (Depth != 0) hash ^= Depth.GetHashCode();
      hash ^= nodeChildren_.GetHashCode();
      if (NodeData.Length != 0) hash ^= NodeData.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (TypeUrl.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(TypeUrl);
      }
      if (Width != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Width);
      }
      if (Height != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Height);
      }
      if (Depth != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Depth);
      }
      nodeChildren_.WriteTo(output, _repeated_nodeChildren_codec);
      if (NodeData.Length != 0) {
        output.WriteRawTag(50);
        output.WriteBytes(NodeData);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (TypeUrl.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TypeUrl);
      }
      if (Width != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Width);
      }
      if (Height != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Height);
      }
      if (Depth != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Depth);
      }
      size += nodeChildren_.CalculateSize(_repeated_nodeChildren_codec);
      if (NodeData.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(NodeData);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SparseVoxelOctree other) {
      if (other == null) {
        return;
      }
      if (other.TypeUrl.Length != 0) {
        TypeUrl = other.TypeUrl;
      }
      if (other.Width != 0) {
        Width = other.Width;
      }
      if (other.Height != 0) {
        Height = other.Height;
      }
      if (other.Depth != 0) {
        Depth = other.Depth;
      }
      nodeChildren_.Add(other.nodeChildren_);
      if (other.NodeData.Length != 0) {
        NodeData = other.NodeData;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            TypeUrl = input.ReadString();
            break;
          }
          case 16: {
            Width = input.ReadInt32();
            break;
          }
          case 24: {
            Height = input.ReadInt32();
            break;
          }
          case 32: {
            Depth = input.ReadInt32();
            break;
          }
          case 42:
          case 40: {
            nodeChildren_.AddEntriesFrom(input, _repeated_nodeChildren_codec);
            break;
          }
          case 50: {
            NodeData = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code