using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iGadget.Util.MIME
{
	[XmlRoot("mime-types", IsNullable = false)]
	public class MIMETypes
	{
		private static MIMETypes Loaded { get; set; }

		private MIMETypes()
		{
			Types = new List<MIMEType>();
		}

		public static MIMETypes Load()
		{
			if (Loaded == null)
			{
				var type = typeof(MIMETypes);
				var assembly = type.Assembly;
				using (var stream = assembly.GetManifestResourceStream(type.Namespace + ".mime-types.xml"))
				{
					var serializer = new XmlSerializer(type);
					if (stream != null)
					{
						Loaded = (MIMETypes)serializer.Deserialize(stream);
					}
				}
			}
			return Loaded;
		}

		[XmlElement("mime-type")]
		public List<MIMEType> Types { get; set; }

		public string GetRecommendedExtension(byte[] data)
		{
			var mimeType = Types.FirstOrDefault(a => a.Matches(data));
			return mimeType == null ? "bin" : (mimeType.Extensions.FirstOrDefault() ?? "bin");
		}

		public string GetMIMEType(byte[] data)
		{
			var mimeType = Types.FirstOrDefault(a => a.Matches(data));

			if (mimeType == null)
			{
				return data.All(a => a > 0x20 && a < 127) ? "text/plain" : "application/octet-stream";
			}

			return mimeType.Name;
		}
	}

	[XmlRoot("mime-type", IsNullable = false)]
	public class MIMEType
	{
		public MIMEType()
		{
			Extensions = new List<string>();
			MagicNumbers = new List<Magic>();
		}

		[XmlElement("ext")]
		public List<string> Extensions { get; set; }

		[XmlElement("magic")]
		public List<Magic> MagicNumbers { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("description")]
		public string Description { get; set; }

		public bool Matches(string fileName)
		{
			return Extensions.Any(a => a.Equals(Path.GetExtension(fileName).Substring(1)));
		}

		public bool Matches(byte[] data)
		{
			return MagicNumbers.Any(a => a.Matches(data));
		}
	}

	[XmlRoot("magic", IsNullable = false)]
	public class Magic
	{
		public Magic()
		{
			Type = MagicType.@string;
		}

		private byte[] parsed { get; set; }

		[XmlAttribute("offset")]
		public int Offset { get; set; }

		[XmlAttribute("type"), DefaultValue(MagicType.@string)]
		public MagicType Type { get; set; }

		[XmlAttribute("value")]
		public string Value { get; set; }

		public bool Matches(byte[] data)
		{
			if (data.Length <= Offset)
			{
				return false;
			}

			if (parsed == null)
			{
				switch (Type)
				{
					case MagicType.@byte:
						parsed = StringToByteArray(Value);
						break;
					case MagicType.@string:
						parsed = Encoding.UTF8.GetBytes(Value);
						break;
					default:
						return false;
				}
			}

			return data.Length >= Offset + parsed.Length && data.Skip(Offset).Take(parsed.Length).SequenceEqual(parsed);
		}

		private static byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length / 2).Select(x => Byte.Parse(hex.Substring(2 * x, 2), NumberStyles.HexNumber)).ToArray();
		}
	}

	[XmlType(AnonymousType = true, Namespace = "http://tempuri.org/mime-types")]
	public enum MagicType
	{
		@byte,
		@string,
	}
}